using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Maintenance;
using SmartMaint.Api.Modules.Cases.Dtos;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>
/// Meeting 07/11, the staff's side of a case: approve with the work order form (technician, planned window, tasks),
/// one Edit instead of change urgency / continue / close / reopen, Reassign (a new technician on taken work = a new
/// case), a repeated request handed to the case it repeats, outside parties marked on the tasks, followers.
/// </summary>
internal sealed partial class CaseService
{
    private const int TaskDescriptionMax = 500;

    private const int FollowerMatches = 20;

    public async Task<FollowerOptionsResponse> GetFollowerOptionsAsync(long id, string? search, CancellationToken cancellationToken)
    {
        var scope = await LoadScopeAsync(cancellationToken);
        var c = await Visible(db.Cases.AsNoTracking(), scope)
            .Where(x => x.Id == id)
            .Select(x => new { x.RequestingCustomerId, Followers = x.Followers.Select(f => f.CustomerId).ToList() })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw CaseErrors.NotFound();

        var customers = db.Customers.AsNoTracking().Where(x => x.Status == UserStatus.Active && x.Id != c.RequestingCustomerId);
        var current = await customers.Where(x => c.Followers.Contains(x.Id))
            .OrderBy(x => x.FullName)
            .Select(x => new FollowerOptionResponse(x.Id, x.FullName, x.Username, x.Email))
            .ToListAsync(cancellationToken);

        var words = search?.Trim().ToLower();
        var matches = await customers
            .Where(x => !c.Followers.Contains(x.Id))
            .Where(x => string.IsNullOrEmpty(words) || x.Username.ToLower().Contains(words)
                        || (x.Email != null && x.Email.ToLower().Contains(words)) || x.FullName.ToLower().Contains(words))
            .OrderBy(x => x.FullName)
            .Take(FollowerMatches)
            .Select(x => new FollowerOptionResponse(x.Id, x.FullName, x.Username, x.Email))
            .ToListAsync(cancellationToken);
        return new FollowerOptionsResponse(current, matches);
    }

    public Task<CaseDetailResponse> SetFollowersAsync(long id, SetFollowersRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.ManageFollowers, async (c, now) =>
        {
            await SyncFollowersAsync(c, request.CustomerIds, now, cancellationToken);
            return null;
        }, cancellationToken);

    public Task<CaseDetailResponse> ReassignAsync(long id, ReassignRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Reassign, async (c, now) =>
        {
            if (request.TechnicianId == c.AssignedTechnicianId)
            {
                throw CaseErrors.SameTechnician();
            }

            if (CaseTransitions.Taken.Contains(c.Status))
            {
                var input = new WorkOrderInput(c.Priority.Code, c.CategoryId, null, null, c.PlannedStartAt, c.PlannedEndAt, null);
                return await ContinueInNewCaseAsync(c, request.TechnicianId, input, now, cancellationToken);
            }

            await ChangeTechnicianAsync(c, request.TechnicianId, cancellationToken);
            return null;
        }, cancellationToken);

    public Task<CaseDetailResponse> ApproveAsync(long id, ApproveCaseRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Approve, async (c, now) =>
        {
            c.StaffId ??= currentUser.Id;
            var notes = await ApplyWorkOrderAsync(
                c,
                new WorkOrderInput(
                    request.Urgency, request.ServiceTypeId ?? c.CategoryId, request.Title, request.Description,
                    request.PlannedStartAt, request.PlannedEndAt, request.Tasks),
                now,
                cancellationToken);
            var technician = await ActiveTechnicianAsync(request.TechnicianId, cancellationToken);
            c.AssignedTechnicianId = technician.Id;
            c.Technician = technician;
            c.AcceptRemindedAt = null;
            return string.Join(' ', [$"Assigned to {technician.FullName}.", .. notes]);
        }, cancellationToken);

    public Task<CaseDetailResponse> RejectAsync(long id, RejectCaseRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Reject, async (c, now) =>
        {
            c.StaffId ??= currentUser.Id;
            var reason = $"{RejectLabels[request.Reason]}: {request.Note.Trim()}";
            if (request.Reason != RejectReason.Duplicate || request.Duplicate is not { } duplicate)
            {
                return reason;
            }

            var target = await HandOverDuplicateAsync(c, duplicate, now, cancellationToken);
            return $"{reason} (see {Case.CodeOf(target.Id)})";
        }, cancellationToken);

    public Task<CaseDetailResponse> EditAsync(long id, EditCaseRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Edit, async (c, now) =>
        {
            if (request.StatusChange is { } change && !StatusChangesOf(c, now).Contains(change))
            {
                throw CaseErrors.StatusChangeNotAllowed();
            }

            // Until the work starts the expected due time is counted from the planned start (meeting note 18/11).
            if (request.PlannedStartAt is null && c.StartedAt is null && c.DueAt is null && request.Urgency != CasePriority.Emergency)
            {
                throw CaseErrors.PlannedStartRequired();
            }

            var input = new WorkOrderInput(
                request.Urgency, request.ServiceTypeId, request.Title, request.Description,
                request.PlannedStartAt, request.PlannedEndAt, request.Tasks);

            var notes = await ApplyWorkOrderAsync(c, input, now, cancellationToken);

            string? reason = null;
            switch (request.StatusChange)
            {
                case EditStatusChange.Continue:
                    c.Status = CaseStatus.InProgress;
                    c.HoldReason = null;
                    c.HoldUntil = null;
                    StartClock(c, now);
                    reason = "The technician continues.";
                    break;
                case EditStatusChange.Close:
                    c.Status = CaseStatus.Closed;
                    c.ClosedAt = now;
                    notifications.Send(CustomersOf(c), $"{Case.CodeOf(c.Id)} \"{c.Title}\" was closed. Please rate the repair.");
                    await NotifyEmergencyClosedAsync(c, await StaffOfBuildingAsync(c.Room.Floor.BuildingId, cancellationToken), cancellationToken);
                    break;
                case EditStatusChange.Reopen:
                    reason = request.Reason!.Trim();
                    ClearForReopen(c, now);
                    c.Status = CaseStatus.Reopened;
                    notifications.Send(
                        [.. CustomersOf(c), .. (c.AssignedTechnicianId is { } t ? new[] { t } : [])],
                        $"{Case.CodeOf(c.Id)} \"{c.Title}\" was reopened: {reason}");
                    break;
            }

            if (request.StatusChange is null && notes.Count > 0 && c.AssignedTechnicianId is { } technicianId)
            {
                notifications.Send([technicianId], $"{Case.CodeOf(c.Id)} \"{c.Title}\" was updated: {string.Join(' ', notes)}");
            }

            // F6 counts the emergencies that were not one (the only edit kept in the history besides status changes).
            return reason ?? notes.FirstOrDefault(CaseHistoryTags.IsCounted);
        }, cancellationToken);

    public Task<CaseDetailResponse> SetTaskThirdPartyAsync(
        long id, long taskId, SetTaskThirdPartyRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.SetTaskThirdParty, (c, now) =>
        {
            var task = c.Tasks.SingleOrDefault(t => t.Id == taskId) ?? throw CaseErrors.TaskNotFound();
            if (request.ThirdParty)
            {
                task.DoneBy = TaskDoneBy.ThirdParty;
                task.Status = CaseTaskStatus.Pass;
                task.DoneAt = now;
            }
            else if (task.DoneBy == TaskDoneBy.ThirdParty)
            {
                // Back to the technician.
                task.DoneBy = null;
                task.Status = CaseTaskStatus.Pending;
                task.DoneAt = null;
            }

            return Task.FromResult<string?>(null);
        }, cancellationToken);

    public Task<CaseDetailResponse> OutsourceAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Outsource, (c, now) =>
        {
            // Meeting 07/11: agreed with the party outside the system; the staff then marks the tasks it does.
            StopClock(c, now);
            c.HoldReason = HoldReason.WaitingThirdParty;
            c.HoldUntil = null;
            c.OutsourcedAt ??= now;
            var text = $"{Case.CodeOf(c.Id)} \"{c.Title}\" was handed to an outside party.";
            notifications.Send(c.AssignedTechnicianId is { } t ? [t] : [], text);
            notifications.Send(CustomersOf(c), text);
            return Task.FromResult<string?>("Handed to an outside party.");
        }, cancellationToken);

    /// <summary>The status changes the Edit form may offer for this case now (computed here, drawn by the web).</summary>
    internal static List<EditStatusChange> StatusChangesOf(Case c, DateTimeOffset now) => c.Status switch
    {
        CaseStatus.OnHold => [EditStatusChange.Continue],
        CaseStatus.Completed => [EditStatusChange.Close, EditStatusChange.Reopen],
        _ => [],
    };

    // ----- helpers ----------------------------------------------------------------------------------------------

    private sealed record WorkOrderInput(
        CasePriority Urgency,
        long ServiceTypeId,
        string? Title,
        string? Description,
        DateTimeOffset? PlannedStartAt,
        DateTimeOffset? PlannedEndAt,
        IReadOnlyList<TaskLineRequest>? Tasks);

    /// <summary>
    /// The fields the approve and edit forms share. Returns what changed in words (category, urgency) for the history
    /// reason or the technician's notification.
    /// </summary>
    private async Task<List<string>> ApplyWorkOrderAsync(Case c, WorkOrderInput input, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var notes = new List<string>();
        if (!string.IsNullOrWhiteSpace(input.Title))
        {
            c.Title = input.Title.Trim();
        }

        if (!string.IsNullOrWhiteSpace(input.Description))
        {
            c.Description = input.Description.Trim();
        }

        if (input.ServiceTypeId != c.CategoryId)
        {
            var category = await db.CaseCategories
                .SingleOrDefaultAsync(x => x.Id == input.ServiceTypeId && x.ArchivedAt == null, cancellationToken)
                ?? throw CaseErrors.UnknownServiceType();
            if (category.AssetTypeId is { } typeId && c.AssetId is { } assetId
                && !await db.Assets.AnyAsync(a => a.Id == assetId && a.TypeId == typeId, cancellationToken))
            {
                throw CaseErrors.FaultNotForAsset();
            }

            notes.Add($"Category corrected from {CaseMapper.ServiceTypeName(c.Category)} to {CaseMapper.ServiceTypeName(category)}.");
            c.CategoryId = category.Id;
            c.Category = category;
        }

        notes.AddRange(await ApplyUrgencyAsync(c, input.Urgency, now, cancellationToken));
        c.PlannedStartAt = input.PlannedStartAt?.ToUniversalTime();
        c.PlannedEndAt = input.PlannedEndAt?.ToUniversalTime();
        SyncTasks(c, input.Tasks, keepFinishedOf: null);
        return notes;
    }

    /// <summary>
    /// The urgency, with what it means for the clocks: Emergency enters F4 (24/7 from now); leaving Emergency goes back
    /// to F3 (counted from the start of work); another level moves the deadline from the start of work (O08).
    /// </summary>
    private async Task<List<string>> ApplyUrgencyAsync(Case c, CasePriority urgency, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var notes = new List<string>();
        var before = c.Priority;
        var priority = before.Code == urgency
            ? before
            : await db.PriorityLevels.SingleAsync(p => p.Code == urgency, cancellationToken);
        c.PriorityLevelId = priority.Id;
        c.Priority = priority;
        if (priority.Id != before.Id)
        {
            notes.Add($"Urgency {before.Name} → {priority.Name}.");
        }

        if (urgency == CasePriority.Emergency)
        {
            if (c.EmergencyAt is null)
            {
                // The staff decided it: already acknowledged, nobody to chase.
                EnterEmergency(c, priority, now, acknowledged: true);
                notes.Add("Handed over to F4.");
            }

            return notes;
        }

        if (c.EmergencyAt is not null)
        {
            // K04: not an emergency after all.
            c.EmergencyAt = null;
            c.AcknowledgedAt ??= now;
            notes.Add($"{CaseHistoryTags.NotAnEmergency}, moved to F3.");
        }

        if (priority.Id != before.Id || notes.Count > 0)
        {
            c.DueAt = c.StartedAt?.AddHours(priority.SlaHours);
            c.SlaWarnedAt = null;
            c.SlaOverdueNotifiedAt = null;
        }

        return notes;
    }

    /// <summary>
    /// Makes the task list match the form: rename / add, remove the ones left out that have no result yet. No task at
    /// all = one task named after the case (every case has tasks, meeting 07/11). <paramref name="keepFinishedOf"/>:
    /// ids that stayed in the previous case and are ignored here.
    /// </summary>
    private void SyncTasks(Case c, IReadOnlyList<TaskLineRequest>? lines, IReadOnlySet<long>? keepFinishedOf)
    {
        if (lines is not null)
        {
            var wanted = lines.Where(l => l.Id is not null && keepFinishedOf?.Contains(l.Id.Value) != true).ToList();
            var keep = wanted.Select(l => l.Id!.Value).ToHashSet();
            foreach (var task in c.Tasks.Where(t => !keep.Contains(t.Id)).ToList())
            {
                if (task.Status != CaseTaskStatus.Pending || task.DoneBy is not null)
                {
                    throw CaseErrors.TaskAlreadyDone(task.Name);
                }

                c.Tasks.Remove(task);
                db.CaseTasks.Remove(task);
            }

            foreach (var line in wanted)
            {
                var task = c.Tasks.SingleOrDefault(t => t.Id == line.Id) ?? throw CaseErrors.TaskNotFound();
                task.Name = line.Name.Trim();
                task.Description = Clean(line.Description);
            }

            foreach (var line in lines.Where(l => l.Id is null))
            {
                c.Tasks.Add(new CaseTask { Name = line.Name.Trim(), Description = Clean(line.Description), Status = CaseTaskStatus.Pending });
            }
        }

        if (c.Tasks.Count == 0)
        {
            c.Tasks.Add(new CaseTask { Name = c.Title, Status = CaseTaskStatus.Pending });
        }
    }

    private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    /// <summary>The followers become exactly <paramref name="customerIds"/>.</summary>
    private async Task SyncFollowersAsync(Case c, IReadOnlyList<long> customerIds, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var wanted = customerIds.Where(x => x != c.RequestingCustomerId).ToHashSet();
        var valid = await db.Customers
            .Where(x => wanted.Contains(x.Id) && x.Status == UserStatus.Active)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        if (valid.Count != wanted.Count)
        {
            throw CaseErrors.InvalidFollower();
        }

        foreach (var follower in c.Followers.Where(f => !wanted.Contains(f.CustomerId)).ToList())
        {
            c.Followers.Remove(follower);
            db.CaseFollowers.Remove(follower);
        }

        foreach (var customerId in wanted.Where(x => c.Followers.All(f => f.CustomerId != x)))
        {
            c.Followers.Add(new CaseFollower { CustomerId = customerId, AddedAt = now });
            notifications.Send([customerId], $"You now follow {Case.CodeOf(c.Id)} \"{c.Title}\".");
        }
    }

    /// <summary>A case nobody has taken yet simply gets the new technician.</summary>
    private async Task ChangeTechnicianAsync(Case c, long technicianId, CancellationToken cancellationToken)
    {
        var before = c.AssignedTechnicianId;
        var code = Case.CodeOf(c.Id);
        var technician = await ActiveTechnicianAsync(technicianId, cancellationToken);
        c.AssignedTechnicianId = technician.Id;
        c.Technician = technician;
        c.AcceptRemindedAt = null;
        notifications.Send([technician.Id], $"{code} \"{c.Title}\" was assigned to you.");

        if (before is { } old)
        {
            notifications.Send([old], $"{code} \"{c.Title}\" was given to another technician.");
        }
    }

    private async Task<Technician> ActiveTechnicianAsync(long id, CancellationToken cancellationToken) =>
        await db.Technicians.SingleOrDefaultAsync(t => t.Id == id && t.Status == UserStatus.Active, cancellationToken)
        ?? throw CaseErrors.InvalidTechnician();

    /// <summary>
    /// Meeting 07/11, Leader 09/11: the old technician already took the work, so a new case continues it. The tasks they
    /// finished stay here and this case closes (their hours and labour count); the unfinished tasks move to the new case,
    /// which links back (PreviousCaseId) and waits for the new technician to accept. The SLA clock keeps running.
    /// </summary>
    private async Task<string> ContinueInNewCaseAsync(
        Case c,
        long technicianId,
        WorkOrderInput input,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var technician = await ActiveTechnicianAsync(technicianId, cancellationToken);
        await db.Entry(c).Collection(x => x.Images).LoadAsync(cancellationToken);

        var next = new Case
        {
            RequestingCustomerId = c.RequestingCustomerId,
            RoomId = c.RoomId,
            Room = c.Room,
            AssetId = c.AssetId,
            CategoryId = c.CategoryId,
            Category = c.Category,
            TriggerWorkOrderId = c.TriggerWorkOrderId,
            Title = c.Title,
            Description = c.Description,
            Status = CaseStatus.Approved,
            PriorityLevelId = c.PriorityLevelId,
            Priority = c.Priority,
            StaffId = c.StaffId ?? currentUser.Id,
            AssignedTechnicianId = technician.Id,
            Technician = technician,
            SubmittedAt = c.SubmittedAt,
            StartedAt = c.StartedAt,
            DueAt = c.DueAt,
            EmergencyAt = c.EmergencyAt,
            AcknowledgedAt = c.AcknowledgedAt,
            MadeSafeAt = c.MadeSafeAt,
            OutsourcedAt = c.OutsourcedAt,
            IsSurvey = c.IsSurvey,
            PreviousCaseId = c.Id,
        };
        foreach (var image in c.Images.Where(i => i.Kind == CaseImageKind.Report))
        {
            next.Images.Add(new CaseImage { Kind = image.Kind, Image = image.Image, Notes = image.Notes });
        }

        foreach (var follower in c.Followers)
        {
            next.Followers.Add(new CaseFollower { CustomerId = follower.CustomerId, AddedAt = now });
        }

        // Meeting note 18/11: the old case keeps its whole task list; the unfinished tasks are marked Moved there and go
        // on as copies (with their notes and photos) in the new case.
        await db.Entry(c).Collection(x => x.Tasks).Query().Include(t => t.Notes).Include(t => t.Images).LoadAsync(cancellationToken);
        var finished = c.Tasks.Where(t => t.Status != CaseTaskStatus.Pending || t.DoneBy is not null).ToList();
        foreach (var task in c.Tasks.Except(finished).ToList())
        {
            task.Status = CaseTaskStatus.Moved;
            next.Tasks.Add(new CaseTask
            {
                Name = task.Name,
                Description = task.Description,
                Status = CaseTaskStatus.Pending,
                Notes = task.Notes.Select(n => new TaskNote { Note = n.Note }).ToList(),
                Images = task.Images.Select(i => new TaskImage { Image = i.Image, Notes = i.Notes }).ToList(),
            });
        }

        await ApplyWorkOrderAsync(next, input with { Tasks = null }, now, cancellationToken);
        SyncTasks(next, input.Tasks, keepFinishedOf: null);
        next.History.Add(new CaseStatusHistory
        {
            NewStatus = CaseStatus.Approved,
            ChangedById = currentUser.Id,
            ChangedAt = now,
            Reason = $"Continues {Case.CodeOf(c.Id)}, assigned to {technician.FullName}.",
        });
        db.Cases.Add(next);
        await db.SaveChangesAsync(cancellationToken);

        // The old case: what its technician did, closed.
        var oldTechnician = c.AssignedTechnicianId;
        c.CompletedAt ??= now;
        c.ClosedAt = now;
        c.HoldReason = null;
        c.HoldUntil = null;
        if (c.StartedAt is not null)
        {
            StopClock(c, now);
            SetLabourCost(c);
        }

        c.Status = CaseStatus.Closed;
        c.ReassignedAt = now;

        var nextCode = Case.CodeOf(next.Id);
        notifications.Send([technician.Id], $"{nextCode} \"{next.Title}\" was assigned to you (continues {Case.CodeOf(c.Id)}).");
        notifications.Send(oldTechnician is { } old ? [old] : [], $"{Case.CodeOf(c.Id)} \"{c.Title}\" was given to {technician.FullName} as {nextCode}.");
        notifications.Send(CustomersOf(next), $"{Case.CodeOf(c.Id)} \"{c.Title}\" continues as {nextCode} with another technician.");
        return $"Reassigned to {technician.FullName}: continued in {nextCode}.";
    }

    /// <summary>
    /// A repeated request (meeting 07/11, replaces merge): its content may become a task of the case it repeats, its photos
    /// may be copied there, and its customer may follow that case. The caller then rejects this request as a duplicate.
    /// </summary>
    private async Task<Case> HandOverDuplicateAsync(Case c, DuplicateOfRequest duplicate, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var target = await WithRefs().SingleOrDefaultAsync(x => x.Id == duplicate.CaseId, cancellationToken);
        if (target is null || target.Id == c.Id || target.IsPreventive
            || target.Room.Floor.BuildingId != c.Room.Floor.BuildingId
            || target.Status is CaseStatus.Rejected or CaseStatus.Cancelled or CaseStatus.Closed)
        {
            throw CaseErrors.InvalidDuplicateTarget();
        }

        var code = Case.CodeOf(c.Id);
        var targetCode = Case.CodeOf(target.Id);

        // Meeting note 18/11: the repeat can still be opened, but it counts as one problem with the case it repeats.
        c.MergedIntoCaseId = target.Id;
        if (duplicate.AddAsTask)
        {
            var description = $"From {code}: {c.Description}";
            target.Tasks.Add(new CaseTask
            {
                Name = c.Title,
                Description = description.Length > TaskDescriptionMax ? description[..TaskDescriptionMax] : description,
                Status = CaseTaskStatus.Pending,
            });
            notifications.Send(target.AssignedTechnicianId is { } t ? [t] : [], $"{targetCode} \"{target.Title}\" has a new task: {c.Title}.");
        }

        if (duplicate.CopyPhotos)
        {
            await db.Entry(c).Collection(x => x.Images).LoadAsync(cancellationToken);
            foreach (var image in c.Images.Where(i => i.Kind == CaseImageKind.Report))
            {
                target.Images.Add(new CaseImage { Kind = CaseImageKind.Report, Image = image.Image, Notes = $"From {code}" });
            }
        }

        if (duplicate.AddFollower && c.RequestingCustomerId is { } customerId
            && customerId != target.RequestingCustomerId && target.Followers.All(f => f.CustomerId != customerId))
        {
            target.Followers.Add(new CaseFollower { CustomerId = customerId, AddedAt = now });
        }

        return target;
    }
}
