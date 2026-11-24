using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Common.Storage;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Maintenance;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.Api.Modules.Notifications;

namespace SmartMaint.Api.Modules.Cases;

internal sealed partial class CaseService(
    AppDbContext db,
    ICurrentUser currentUser,
    IFileStorage storage,
    INotificationService notifications,
    TimeProvider clock) : ICaseService
{
    /// <summary>The customer may report "not fixed" / the staff member may reopen a closed case for this long (F3 step 12).</summary>
    private static readonly TimeSpan ReopenWindow = TimeSpan.FromDays(7);

    private const string PhotoFolder = "cases";
    private const string ReceiptFolder = "receipts";
    private const string LabourLine = "Labour";

    private static readonly Dictionary<RejectReason, string> RejectLabels = new()
    {
        [RejectReason.MissingInfo] = "Missing information",
        [RejectReason.OutOfScope] = "Outside the building's service scope",
        [RejectReason.Duplicate] = "A case for this problem already exists",
    };

    private static readonly Dictionary<ExtendDueReason, string> ExtendLabels = new()
    {
        [ExtendDueReason.WaitingParts] = "waiting for parts",
        [ExtendDueReason.CustomerRescheduled] = "the customer asked for another time",
        [ExtendDueReason.LargerThanExpected] = "the work is larger than expected",
        [ExtendDueReason.Other] = "other reason",
    };

    // ----- reads ------------------------------------------------------------------------------------------------

    public async Task<PagedResult<CaseSummaryResponse>> ListAsync(PageRequest page, CancellationToken cancellationToken)
    {
        var scope = await LoadScopeAsync(cancellationToken);
        var query = Visible(WithRefs().AsNoTracking().AsSplitQuery(), scope);

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(c => c.SubmittedAt).ThenByDescending(c => c.Id)
            .Skip(page.Skip).Take(page.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CaseSummaryResponse>(rows.Select(CaseMapper.ToSummary).ToList(), page.Page, page.PageSize, total);
    }

    public async Task<CaseDetailResponse> GetAsync(long id, CancellationToken cancellationToken)
    {
        var scope = await LoadScopeAsync(cancellationToken);
        var c = await Visible(WithDetail().AsNoTracking(), scope).SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw CaseErrors.NotFound();

        var photoUrls = await storage.GetUrlsAsync(
            [
                .. c.Images.Select(i => i.Image),
                .. c.Tasks.SelectMany(t => t.Images).Select(i => i.Image),
                .. c.Costs.Where(x => x.ReceiptImage != null).Select(x => x.ReceiptImage!),
                .. c.ThirdParties.Where(t => t.EvidenceImage != null).Select(t => t.EvidenceImage!),
            ],
            cancellationToken);
        var mergedCaseIds = await db.Cases.Where(x => x.MergedIntoCaseId == c.Id).OrderBy(x => x.Id).Select(x => x.Id)
            .ToListAsync(cancellationToken);
        var nextCaseId = await db.Cases.Where(x => x.PreviousCaseId == c.Id).Select(x => (long?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var now = clock.GetUtcNow();
        var allowed = AllowedActions(c, scope, now);
        var extras = new CaseMapper.DetailExtras(
            mergedCaseIds,
            scope.Role is UserRole.Staff or UserRole.Manager ? await DecisionInfoAsync(c, cancellationToken) : null,
            nextCaseId,
            allowed.Contains(CaseAction.Edit) ? StatusChangesOf(c, now) : []);
        // Meeting 07/11: how the work is done (tasks, costs, repair record) is for the building's people only.
        return CaseMapper.ToDetail(c, allowed, photoUrls, showWork: scope.Role != UserRole.Customer, extras);
    }

    // ----- create -----------------------------------------------------------------------------------------------

    public async Task<CaseDetailResponse> CreateAsync(CreateCaseRequest request, CancellationToken cancellationToken)
    {
        var category = await db.CaseCategories
            .SingleOrDefaultAsync(x => x.Id == request.ServiceTypeId && x.ArchivedAt == null, cancellationToken)
            ?? throw CaseErrors.UnknownServiceType();

        var room = await db.Rooms.Include(r => r.Floor)
            .SingleOrDefaultAsync(r => r.Id == request.RoomId && r.DeletedAt == null, cancellationToken)
            ?? throw CaseErrors.UnknownRoom();

        // Meeting note 18/11: any room of any building, but always about one asset in service there (Outscope 23).
        var asset = await db.Assets.AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == request.AssetId && a.RoomId == room.Id && a.RetiredAt == null, cancellationToken)
            ?? throw CaseErrors.AssetNotInRoom();
        if (category.AssetTypeId is { } typeId && typeId != asset.TypeId)
        {
            throw CaseErrors.FaultNotForAsset();
        }

        // The customer's choice. Even an emergency waits in Pending until the staff approve it (meeting note 18/11).
        var urgency = request.Urgency ?? CasePriority.Normal;
        var priority = await db.PriorityLevels.SingleAsync(p => p.Code == urgency, cancellationToken);

        var now = clock.GetUtcNow();
        var c = new Case
        {
            RequestingCustomerId = currentUser.Id,
            RoomId = room.Id,
            AssetId = asset.Id,
            CategoryId = category.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = CaseStatus.Pending,
            PriorityLevelId = priority.Id,
            SubmittedAt = now,
        };

        await AddPhotosAsync(c, CaseImageKind.Report, request.Photos ?? [], cancellationToken);
        c.History.Add(new CaseStatusHistory { NewStatus = c.Status, ChangedById = currentUser.Id, ChangedAt = now });

        db.Cases.Add(c);
        await db.SaveChangesAsync(cancellationToken);

        // The code exists only after the first save. 5.4.2: every staff member of the building (N11).
        var where = $"{room.Floor.DisplayName}, {room.RoomNumber}";
        var kind = urgency == CasePriority.Emergency ? "EMERGENCY request" : "New request";
        notifications.Send(
            await StaffOfBuildingAsync(room.Floor.BuildingId, cancellationToken),
            $"{kind} {Case.CodeOf(c.Id)} \"{c.Title}\" in {where}.");

        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(c.Id, cancellationToken);
    }

    // ----- status changes (one method per arrow of the state machine) ---------------------------------------------
    // Approve, Reject, Edit and the task / follower steps of the staff are in CaseService.WorkOrder.cs.

    public Task<CaseDetailResponse> CancelAsync(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Cancel, (c, now) =>
        {
            StopThirdParties(c, now);
            StopClock(c, now);
            return Task.FromResult<string?>(request.Reason.Trim());
        }, cancellationToken);

    public Task<CaseDetailResponse> AcceptAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Accept, (_, _) => Task.FromResult<string?>(null), cancellationToken);

    public Task<CaseDetailResponse> DeclineAsync(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Decline, (c, now) =>
        {
            // ERD: TechnicianRejections. The manager counts these declines (N10).
            c.Rejections.Add(new TechnicianRejection
            {
                TechnicianId = currentUser.Id,
                RejectedAt = now,
                Reason = request.Reason.Trim(),
            });
            // Back to the staff member (status Approved, nobody named) who chooses someone else with Edit.
            c.AssignedTechnicianId = null;
            c.Technician = null;
            return Task.FromResult<string?>(request.Reason.Trim());
        }, cancellationToken);

    // Meeting 07/11: the technician moves the status with one dropdown (In progress / On hold / Completed). In progress
    // runs the work clock; On hold and Completed stop it. Nothing else is asked (no hold reason, no repair record).

    public Task<CaseDetailResponse> StartAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Start, (c, now) =>
        {
            // A case that continues another one keeps that one's SLA clock.
            c.StartedAt ??= now;
            c.DueAt ??= c.StartedAt.Value.AddHours(c.Priority.SlaHours);
            StartClock(c, now);
            return Task.FromResult<string?>(null);
        }, cancellationToken);

    public Task<CaseDetailResponse> HoldAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Hold, (c, now) =>
        {
            StopClock(c, now);
            c.HoldReason = null;
            c.HoldUntil = null;
            return Task.FromResult<string?>(null);
        }, cancellationToken);

    public Task<CaseDetailResponse> ResumeAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Resume, (c, now) =>
        {
            c.HoldReason = null;
            c.HoldUntil = null;
            StartClock(c, now);
            return Task.FromResult<string?>(null);
        }, cancellationToken);

    public Task<CaseDetailResponse> CompleteAsync(long id, CompleteWorkRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Complete, (c, now) =>
        {
            StopClock(c, now);
            c.CompletedAt = now;
            SetLabourCost(c);
            return Task.FromResult<string?>(null);
        }, cancellationToken);

    // ----- work hours and the technician's costs (meeting 07/11) -----------------------------------------------

    public Task<CaseDetailResponse> LogHoursAsync(long id, LogHoursRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.LogHours, (c, now) =>
        {
            c.ActualCompletionHours = Math.Round(request.Hours, 2);
            // A running clock goes on from the typed total.
            if (c.ClockStartedAt is not null)
            {
                c.ClockStartedAt = now;
            }

            return Task.FromResult<string?>(null);
        }, cancellationToken);

    public Task<CaseDetailResponse> AddCostAsync(long id, AddCostRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.AddCost, async (c, _) =>
        {
            var receipt = string.IsNullOrWhiteSpace(request.ReceiptPhoto)
                ? null
                : await storage.SaveAsync(ReceiptFolder, DataUrl.ParseImage(request.ReceiptPhoto), cancellationToken);
            c.Costs.Add(new Cost
            {
                Name = request.Name.Trim(),
                Description = Clean(request.Description),
                Amount = request.Amount,
                ReceiptCode = Clean(request.ReceiptCode),
                ReceiptImage = receipt,
                AddedById = currentUser.Id,
            });
            notifications.Send(
                c.StaffId is { } s ? [s] : await StaffOfBuildingAsync(c.Room.Floor.BuildingId, cancellationToken),
                $"{c.Technician?.FullName} added a cost to {Case.CodeOf(c.Id)}: {request.Name.Trim()}, {request.Amount:N0} VND.");
            return null;
        }, cancellationToken);

    public Task<CaseDetailResponse> EditCostAsync(long id, long costId, EditCostRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.EditOwnCost, async (c, _) =>
        {
            var cost = c.Costs.SingleOrDefault(x => x.Id == costId) ?? throw CaseErrors.CostNotFound();
            if (cost.AddedById != currentUser.Id)
            {
                throw CaseErrors.CostNotYours();
            }

            var code = Clean(request.ReceiptCode);
            if (code is null && cost.ReceiptImage is null && string.IsNullOrWhiteSpace(request.ReceiptPhoto))
            {
                throw CaseErrors.ReceiptRequired();
            }

            if (!string.IsNullOrWhiteSpace(request.ReceiptPhoto))
            {
                cost.ReceiptImage = await storage.SaveAsync(ReceiptFolder, DataUrl.ParseImage(request.ReceiptPhoto), cancellationToken);
            }

            cost.Name = request.Name.Trim();
            cost.Description = Clean(request.Description);
            cost.Amount = request.Amount;
            cost.ReceiptCode = code;
            return null;
        }, cancellationToken);

    /// <summary>The work clock runs from now (In progress).</summary>
    private static void StartClock(Case c, DateTimeOffset now) => c.ClockStartedAt ??= now;

    /// <summary>Adds the running stretch to the work hours and stops the clock.</summary>
    private static void StopClock(Case c, DateTimeOffset now)
    {
        if (c.ClockStartedAt is not { } started)
        {
            return;
        }

        var stretch = (decimal)(now - started).TotalHours;
        c.ActualCompletionHours = Math.Max(0.01m, Math.Round((c.ActualCompletionHours ?? 0) + stretch, 2));
        c.ClockStartedAt = null;
    }

    /// <summary>One labour line = work hours × the technician's hourly rate (E9); replaced when the case is completed again.</summary>
    private void SetLabourCost(Case c)
    {
        var old = c.Costs.Where(x => x.Name == LabourLine && x.ReceiptCode == null && x.ReceiptImage == null).ToList();
        foreach (var line in old)
        {
            c.Costs.Remove(line);
            db.Costs.Remove(line);
        }

        if (c.Technician?.HourlyRate is { } rate && rate > 0 && c.ActualCompletionHours is { } hours)
        {
            c.Costs.Add(new Cost
            {
                Name = LabourLine,
                Description = $"{c.Technician.FullName}: {hours:0.##} h × {rate:N0} VND",
                Amount = Math.Round(hours * rate, 0),
            });
        }
    }

    // ----- tasks ------------------------------------------------------------------------------------------------

    public Task<CaseDetailResponse> UpdateTaskAsync(
        long id, long taskId, UpdateTaskRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.UpdateTask, async (c, now) =>
        {
            var task = await db.CaseTasks.Include(t => t.Images).SingleOrDefaultAsync(t => t.Id == taskId && t.CaseId == c.Id, cancellationToken)
                ?? throw CaseErrors.TaskNotFound();
            if (task.DoneBy == TaskDoneBy.ThirdParty)
            {
                // Meeting 07/11: the staff marked it as done by an outside party.
                throw CaseErrors.TaskByThirdParty(task.Name);
            }

            var photos = request.Photos ?? [];
            if (request.Status == CaseTaskStatus.Fail && photos.Count == 0 && task.Images.Count == 0)
            {
                // TEC-05: a failed item needs a photo.
                throw CaseErrors.PhotoRequired(task.Name);
            }

            task.Status = request.Status;
            task.DoneBy = request.Status == CaseTaskStatus.Pending ? null : TaskDoneBy.Technician;
            task.DoneAt = request.Status == CaseTaskStatus.Pending ? null : now;
            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                task.Notes.Add(new TaskNote { Note = request.Note.Trim() });
            }

            // Meeting 07/11: each photo may carry a caption (ERD: TaskImages.Notes).
            foreach (var photo in photos)
            {
                var path = await storage.SaveAsync("tasks", DataUrl.ParseImage(photo.Image), cancellationToken);
                task.Images.Add(new TaskImage { Image = path, Notes = Clean(photo.Caption) });
            }

            if (request.Status != CaseTaskStatus.Fail || request.FailAction != FailAction.CreateCase || task.RepairCaseId is not null)
            {
                return null;
            }

            // BR-07: a repair case from the failed item — F3 at Approved for the staff to name a technician (Edit),
            // F4 when dangerous.
            var urgency = request.Dangerous ? CasePriority.Emergency : CasePriority.Normal;
            var priority = await db.PriorityLevels.SingleAsync(p => p.Code == urgency, cancellationToken);
            var repair = new Case
            {
                RoomId = c.RoomId,
                AssetId = c.AssetId,
                CategoryId = c.CategoryId,
                Title = $"Repair: {task.Name}",
                Description = $"{request.Note?.Trim()}\nFound during work order {Case.CodeOf(c.Id)}.",
                Status = CaseStatus.Approved,
                PriorityLevelId = priority.Id,
                StaffId = c.StaffId,
                SubmittedAt = now,
            };
            repair.Tasks.Add(new CaseTask { Name = repair.Title, Status = CaseTaskStatus.Pending });
            if (request.Dangerous)
            {
                // The staff still has to acknowledge it: the watch job escalates if nobody does.
                EnterEmergency(repair, priority, now, acknowledged: false);
            }
            repair.History.Add(new CaseStatusHistory
            {
                NewStatus = CaseStatus.Approved,
                ChangedById = currentUser.Id,
                ChangedAt = now,
                Reason = $"Failed task \"{task.Name}\" of {Case.CodeOf(c.Id)}.",
            });
            db.Cases.Add(repair);
            await db.SaveChangesAsync(cancellationToken);
            task.RepairCaseId = repair.Id;

            var kind = request.Dangerous ? "Emergency repair" : "Repair";
            notifications.Send(
                await StaffOfBuildingAsync(c.Room.Floor.BuildingId, cancellationToken),
                $"{kind} case {Case.CodeOf(repair.Id)} \"{repair.Title}\" was made from {Case.CodeOf(c.Id)}. Choose a technician.");
            return null;
        }, cancellationToken);

    /// <summary>
    /// BR-04: by default the same technician (still on the case) gets it back and must accept again.
    /// The SLA starts over with the next "start"; the old times stay in the status history.
    /// An emergency keeps counting 24/7: its clock restarts now (F4 page 4, back to step ⑦).
    /// </summary>
    private static void ClearForReopen(Case c, DateTimeOffset now)
    {
        c.StartedAt = null;
        c.DueAt = null;
        c.CompletedAt = null;
        c.ClosedAt = null;
        c.NotFixedReason = null;
        c.NotFixedAt = null;
        c.SlaWarnedAt = null;
        c.SlaOverdueNotifiedAt = null;
        c.AcceptRemindedAt = null;
        if (c.EmergencyAt is not null)
        {
            c.MadeSafeAt = null;
            EnterEmergency(c, c.Priority, now, acknowledged: true);
        }
    }

    // ----- the deadline, "not fixed", costs ---------------------------------------------------------------------

    public Task<CaseDetailResponse> ExtendDueAsync(long id, ExtendDueRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.ExtendDue, (c, _) =>
        {
            if (request.NewDueAt <= c.DueAt)
            {
                throw CaseErrors.DueNotLater();
            }

            c.DueAt = request.NewDueAt.ToUniversalTime();
            c.SlaWarnedAt = null;
            c.SlaOverdueNotifiedAt = null;
            var note = string.IsNullOrWhiteSpace(request.Note) ? string.Empty : $" {request.Note.Trim()}";
            return Task.FromResult<string?>(
                $"{CaseHistoryTags.DeadlineMoved} to {request.NewDueAt:yyyy-MM-dd HH:mm} UTC: {ExtendLabels[request.Reason]}.{note}");
        }, cancellationToken);

    public Task<CaseDetailResponse> ReportNotFixedAsync(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.ReportNotFixed, (c, now) =>
        {
            c.NotFixedReason = request.Reason.Trim();
            c.NotFixedAt = now;
            return Task.FromResult<string?>($"Customer reports it is not fixed: {c.NotFixedReason}");
        }, cancellationToken);

    public Task<CaseDetailResponse> AcceptNotFixedAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.AcceptNotFixed, (c, now) =>
        {
            var reason = $"Not-fixed report accepted: {c.NotFixedReason}";
            ClearForReopen(c, now);
            return Task.FromResult<string?>(reason);
        }, cancellationToken);

    public Task<CaseDetailResponse> ReopenAsync(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Reopen, (c, now) =>
        {
            var reason = request.Reason.Trim();
            ClearForReopen(c, now);
            notifications.Send(
                [.. CustomersOf(c), .. (c.AssignedTechnicianId is { } t ? new[] { t } : [])],
                $"{Case.CodeOf(c.Id)} \"{c.Title}\" was reopened: {reason}");
            return Task.FromResult<string?>(reason);
        }, cancellationToken);

    public Task<CaseDetailResponse> DeclineNotFixedAsync(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.DeclineNotFixed, (c, _) =>
        {
            c.NotFixedReason = null;
            c.NotFixedAt = null;
            return Task.FromResult<string?>($"Not-fixed report declined: {request.Reason.Trim()}");
        }, cancellationToken);

    public Task<CaseDetailResponse> UpdateCostsAsync(long id, UpdateCostsRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.UpdateCosts, (c, _) =>
        {
            // The list becomes the form's lines: kept lines (by id) keep their receipt, the others are new.
            var keep = request.Lines.Where(l => l.Id is not null).Select(l => l.Id!.Value).ToHashSet();
            foreach (var line in c.Costs.Where(x => !keep.Contains(x.Id)).ToList())
            {
                c.Costs.Remove(line);
                db.Costs.Remove(line);
            }

            foreach (var line in request.Lines)
            {
                var cost = line.Id is { } costId
                    ? c.Costs.SingleOrDefault(x => x.Id == costId) ?? throw CaseErrors.CostNotFound()
                    : new Cost { AddedById = currentUser.Id };
                cost.Name = line.Name.Trim();
                cost.Description = Clean(line.Description);
                cost.Amount = line.Amount;
                if (line.Id is null)
                {
                    c.Costs.Add(cost);
                }
            }

            return Task.FromResult<string?>(null);
        }, cancellationToken);

    public Task<CaseDetailResponse> RateAsync(long id, RateCaseRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Rate, (c, now) =>
        {
            c.RatingStars = request.Stars;
            c.RatingComment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();
            c.RatedAt = now;
            return Task.FromResult<string?>(null);
        }, cancellationToken);

    // ----- the one place a status changes -----------------------------------------------------------------------

    /// <summary>
    /// Loads the case inside the caller's scope, checks the action against the state machine, runs <paramref name="apply"/>
    /// (which changes the fields and returns the reason for the history, if any), moves the status and writes one history
    /// row when the status changed. Meeting 07/11: only status changes are kept; the exceptions are the F4 emergency log
    /// (its lines are its content) and the rows F6 counts (CaseHistoryTags.IsCounted).
    /// </summary>
    private async Task<CaseDetailResponse> ActAsync(
        long id,
        CaseAction action,
        Func<Case, DateTimeOffset, Task<string?>> apply,
        CancellationToken cancellationToken)
    {
        var scope = await LoadScopeAsync(cancellationToken);
        var c = await Visible(WithRefs(), scope).SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw CaseErrors.NotFound();

        var now = clock.GetUtcNow();
        if (!IsAllowed(c, action, scope, now))
        {
            throw CaseErrors.InvalidTransition();
        }

        // Read before apply: a decline clears the technician; Edit may move the status itself.
        var technicianBefore = c.AssignedTechnicianId;
        var statusBefore = c.Status;
        var rule = CaseTransitions.Find(action, scope.Role, c.Status)!;
        var reason = await apply(c, now);

        c.Status = rule.To ?? c.Status;
        var kept = reason is not null && (action == CaseAction.AddLog || CaseHistoryTags.IsCounted(reason));
        if (c.Status != statusBefore || kept)
        {
            c.History.Add(new CaseStatusHistory
            {
                NewStatus = c.Status,
                ChangedById = scope.UserId,
                ChangedAt = now,
                Reason = reason,
            });
        }

        await NotifyAsync(c, action, reason, technicianBefore, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    // ----- notifications (SRS 5.4.2, in-app only) ---------------------------------------------------------------

    /// <summary>The requester and every follower (meeting 07/11).</summary>
    private static long[] CustomersOf(Case c) =>
        [.. (c.RequestingCustomerId is { } r ? new[] { r } : []), .. c.Followers.Select(f => f.CustomerId)];

    /// <summary>Who hears about each arrow. The person who acted is never notified (NotificationService skips them).</summary>
    private async Task NotifyAsync(
        Case c, CaseAction action, string? reason, long? technicianBefore, CancellationToken cancellationToken)
    {
        var code = Case.CodeOf(c.Id);
        var customer = CustomersOf(c);
        var technician = c.AssignedTechnicianId is { } technicianId ? new[] { technicianId } : [];
        var staffOfBuilding = await StaffOfBuildingAsync(c.Room.Floor.BuildingId, cancellationToken);

        switch (action)
        {
            case CaseAction.Approve:
                notifications.Send(customer, $"{code} \"{c.Title}\" was approved and given to a technician.");
                notifications.Send(technician, CaseMapper.IsEmergencyFlow(c)
                    ? $"EMERGENCY {code} \"{c.Title}\" in {c.Room.Floor.DisplayName}, {c.Room.RoomNumber} was assigned to you. Accept it now."
                    : $"{code} \"{c.Title}\" was assigned to you.");
                break;
            case CaseAction.Reject:
                notifications.Send(customer, $"{code} \"{c.Title}\" was rejected. {reason}");
                break;
            case CaseAction.Accept:
                // 5.4.2: the staff member who approved it.
                notifications.Send(c.StaffId is { } staffId ? [staffId] : staffOfBuilding, $"{c.Technician?.FullName} accepted {code}.");
                break;
            case CaseAction.Decline:
                notifications.Send(staffOfBuilding, $"A technician declined {code}: {reason} Choose someone else with Edit.");
                break;
            case CaseAction.Hold:
                notifications.Send([.. staffOfBuilding, .. customer], $"{code} \"{c.Title}\" is on hold. {reason}");
                break;
            case CaseAction.Complete:
                notifications.Send(staffOfBuilding, $"{code} \"{c.Title}\" is done and waits for your check.");
                break;
            case CaseAction.AcceptNotFixed:
                notifications.Send([.. technician, .. customer], $"{code} \"{c.Title}\" was reopened: {reason}");
                break;
            case CaseAction.ExtendDue:
                notifications.Send([.. technician, .. customer], $"{code} \"{c.Title}\": {reason}");
                break;
            case CaseAction.ReportNotFixed:
                notifications.Send(staffOfBuilding, $"{code} \"{c.Title}\": the customer says it is not fixed. Reopen it or answer.");
                break;
            case CaseAction.DeclineNotFixed:
                notifications.Send(customer, $"{code} \"{c.Title}\": {reason}");
                break;
            case CaseAction.Cancel:
                long[] involved = [.. customer, .. (c.StaffId is { } s ? new[] { s } : []), .. (technicianBefore is { } t ? new[] { t } : [])];
                notifications.Send(involved, $"{code} \"{c.Title}\" was cancelled: {reason}");
                break;
            case CaseAction.Edit:
                // EditAsync tells the people concerned itself (who it is depends on what changed).
                break;
            default:
                NotifyEmergency(c, action, staffOfBuilding);
                break;
        }
    }

    /// <summary>Every active staff member assigned to the building (there is no shared queue, N11).</summary>
    private async Task<List<long>> StaffOfBuildingAsync(long buildingId, CancellationToken cancellationToken) =>
        await db.StaffBuildingAssignments
            .Where(a => a.BuildingId == buildingId && a.Staff.Status == UserStatus.Active)
            .Select(a => a.StaffId)
            .ToListAsync(cancellationToken);

    // ----- rules ------------------------------------------------------------------------------------------------

    /// <summary>State machine row + emergency rule + "is this case mine to act on" (B3) + the extra conditions of a few actions.</summary>
    private static bool IsAllowed(Case c, CaseAction action, Scope scope, DateTimeOffset now)
    {
        if (CaseTransitions.Find(action, scope.Role, c.Status) is null)
        {
            return false;
        }

        var emergency = CaseMapper.IsEmergencyFlow(c);
        // A follower only reads the case: acting needs the requester (meeting 07/11).
        var involved = scope.Role switch
        {
            UserRole.Customer => c.RequestingCustomerId == scope.UserId,
            UserRole.Staff => scope.BuildingIds.Contains(c.Room.Floor.BuildingId),
            UserRole.Technician => c.AssignedTechnicianId == scope.UserId,
            _ => false,
        };
        if (!involved)
        {
            return false;
        }

        var reopenable = c.ClosedAt is { } closedAt && now - closedAt <= ReopenWindow;
        return action switch
        {
            CaseAction.Rate => c.RatingStars is null,
            // F2: a preventive work order is done through its checklist, an incident through the repair record.
            // A work order is done when every checklist item has a result (F2); an incident whenever the technician says so.
            CaseAction.Complete => !c.IsPreventive || c.Tasks.All(t => t.Status != CaseTaskStatus.Pending),
            CaseAction.SetTaskThirdParty => c.Tasks.Count > 0,
            // Meeting note 18/11: a closed case is reopened within 7 days, never one closed by a reassign (its work
            // goes on in the new case).
            CaseAction.Reopen => reopenable && c.ReassignedAt is null,
            // O09: only before the deadline passes.
            CaseAction.ExtendDue => c.DueAt is { } due && now < due,
            CaseAction.ReportNotFixed => c.NotFixedAt is null && reopenable && c.ReassignedAt is null,
            CaseAction.AcceptNotFixed => c.NotFixedAt is not null && c.ReassignedAt is null,
            CaseAction.DeclineNotFixed => c.NotFixedAt is not null,
            // F5: while the case waits for the staff's decision or an outside party, the technician does not resume it.
            CaseAction.Resume => c.HoldReason is not (HoldReason.WaitingStaffDecision or HoldReason.WaitingThirdParty),
            CaseAction.RequestDecision => !c.IsPreventive,
            CaseAction.Outsource => !(c.Status == CaseStatus.OnHold && c.HoldReason == HoldReason.WaitingThirdParty),
            CaseAction.Acknowledge => emergency && c.AcknowledgedAt is null,
            CaseAction.AddLog => emergency,
            CaseAction.WriteIncidentReport => emergency && c.EmergencyAt is not null,
            _ => true,
        };
    }

    private static List<CaseAction> AllowedActions(Case c, Scope scope, DateTimeOffset now) =>
        Enum.GetValues<CaseAction>().Where(a => IsAllowed(c, a, scope, now)).ToList();

    private sealed record Scope(long UserId, UserRole Role, IReadOnlyList<long> BuildingIds);

    private async Task<Scope> LoadScopeAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        var role = currentUser.Role;
        IReadOnlyList<long> buildingIds = role switch
        {
            UserRole.Staff => await db.StaffBuildingAssignments
                .Where(a => a.StaffId == userId).Select(a => a.BuildingId).ToListAsync(cancellationToken),
            UserRole.Manager => await db.ManagerBuildingAssignments
                .Where(a => a.ManagerId == userId).Select(a => a.BuildingId).ToListAsync(cancellationToken),
            _ => [],
        };
        return new Scope(userId, role, buildingIds);
    }

    /// <summary>Data scope (B3): which cases may this caller see at all? Admin sees none (BR-11).</summary>
    private static IQueryable<Case> Visible(IQueryable<Case> query, Scope scope)
    {
        var userId = scope.UserId;
        var buildingIds = scope.BuildingIds;
        return scope.Role switch
        {
            // Own requests and the cases the customer follows (meeting 07/11).
            UserRole.Customer => query.Where(c => c.RequestingCustomerId == userId || c.Followers.Any(f => f.CustomerId == userId)),
            UserRole.Staff or UserRole.Manager => query.Where(c => buildingIds.Contains(c.Room.Floor.BuildingId)),
            // A technician who declined still sees the case (read only), so the decline does not "lose" it.
            UserRole.Technician => query.Where(c =>
                c.AssignedTechnicianId == userId || c.Rejections.Any(r => r.TechnicianId == userId)),
            _ => query.Where(_ => false),
        };
    }

    private IQueryable<Case> WithRefs() => db.Cases
        .Include(c => c.Room).ThenInclude(r => r.Floor).ThenInclude(f => f.Building)
        .Include(c => c.Category)
        .Include(c => c.Asset).ThenInclude(a => a!.Type).ThenInclude(t => t.Category)
        .Include(c => c.Priority)
        .Include(c => c.Customer)
        .Include(c => c.Staff)
        .Include(c => c.Technician)
        .Include(c => c.TriggerWorkOrder)
        .Include(c => c.Tasks)
        .Include(c => c.ThirdParties)
        .Include(c => c.Followers)
        .Include(c => c.Costs);

    private IQueryable<Case> WithDetail() => WithRefs()
        .Include(c => c.Asset)
        .Include(c => c.Images)
        .Include(c => c.History).ThenInclude(h => h.ChangedBy)
        .Include(c => c.Rejections).ThenInclude(r => r.Technician)
        .Include(c => c.Tasks).ThenInclude(t => t.Notes)
        .Include(c => c.Tasks).ThenInclude(t => t.Images)
        .Include(c => c.Followers).ThenInclude(f => f.Customer)
        .AsSplitQuery();

    /// <summary>Uploads each data URL to storage (validated already) and links it to the case.</summary>
    private async Task AddPhotosAsync(Case c, CaseImageKind kind, IEnumerable<string> photos, CancellationToken cancellationToken)
    {
        foreach (var photo in photos)
        {
            var path = await storage.SaveAsync(PhotoFolder, DataUrl.ParseImage(photo), cancellationToken);
            c.Images.Add(new CaseImage { Kind = kind, Image = path });
        }
    }

}
