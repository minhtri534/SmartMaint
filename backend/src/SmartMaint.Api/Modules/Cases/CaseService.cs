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

    private static readonly Dictionary<HoldReason, string> HoldLabels = new()
    {
        [HoldReason.WaitingParts] = "Waiting for parts",
        [HoldReason.WaitingCustomer] = "Waiting for the customer",
        [HoldReason.WaitingStaffDecision] = "Waiting for the staff member's decision",
        [HoldReason.WaitingThirdParty] = "Waiting for an outside party",
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
                .. c.ThirdParties.Where(t => t.EvidenceImage != null).Select(t => t.EvidenceImage!),
            ],
            cancellationToken);
        // Costs: staff, managers and the technician; the customer only when the customer pays.
        var showCosts = scope.Role != UserRole.Customer || c.CostBearer == CostBearer.Customer;
        var mergedCaseIds = await db.Cases.Where(x => x.MergedIntoCaseId == c.Id).OrderBy(x => x.Id).Select(x => x.Id)
            .ToListAsync(cancellationToken);
        var extras = new CaseMapper.DetailExtras(
            mergedCaseIds,
            scope.Role is UserRole.Staff or UserRole.Manager ? await DecisionInfoAsync(c, cancellationToken) : null,
            await db.AssetReplacementRequests.AsNoTracking().SingleOrDefaultAsync(r => r.CaseId == c.Id, cancellationToken));
        return CaseMapper.ToDetail(c, AllowedActions(c, scope, clock.GetUtcNow()), photoUrls, showCosts, extras);
    }

    // ----- create -----------------------------------------------------------------------------------------------

    public async Task<CaseDetailResponse> CreateAsync(CreateCaseRequest request, CancellationToken cancellationToken)
    {
        var category = await db.CaseCategories.Include(x => x.DefaultPriority)
            .SingleOrDefaultAsync(x => x.Id == request.ServiceTypeId, cancellationToken)
            ?? throw CaseErrors.UnknownServiceType();

        var room = await db.Rooms.Include(r => r.Floor)
            .SingleOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken)
            ?? throw CaseErrors.UnknownRoom();

        // The customer reports in their own room or in a common area of their building (ERD review #11).
        var home = await db.Customers.AsNoTracking()
            .Where(x => x.Id == currentUser.Id)
            .Select(x => new { x.RoomId, x.Room.Floor.BuildingId })
            .SingleAsync(cancellationToken);
        if (room.Id != home.RoomId && !(room.IsCommonArea && room.Floor.BuildingId == home.BuildingId))
        {
            throw CaseErrors.RoomNotAllowed();
        }

        if (request.AssetId is { } assetId
            && !await db.Assets.AnyAsync(
                a => a.Id == assetId && a.RoomId == request.RoomId && a.RetiredAt == null, cancellationToken))
        {
            throw CaseErrors.AssetNotInRoom();
        }

        var photos = request.Photos ?? [];
        if (category.PhotoRequired && photos.Count == 0)
        {
            throw CaseErrors.PhotoRequired(category.Name);
        }

        // N26: an emergency service type is saved as an emergency case; F4 handles it, F3 offers no actions on it.
        var priority = category.IsEmergency
            ? await db.PriorityLevels.SingleAsync(p => p.Code == CasePriority.Emergency, cancellationToken)
            : category.DefaultPriority;

        var now = clock.GetUtcNow();
        var c = new Case
        {
            RequestingCustomerId = currentUser.Id,
            RoomId = room.Id,
            AssetId = request.AssetId,
            CategoryId = category.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            // F4 page 1 ①: an emergency skips Pending, the system approves it and starts the 24/7 clock.
            Status = category.IsEmergency ? CaseStatus.Approved : CaseStatus.Pending,
            PriorityLevelId = priority.Id,
            CostBearer = category.DefaultCostBearer,
            SubmittedAt = now,
        };
        if (category.IsEmergency)
        {
            EnterEmergency(c, priority, now, acknowledged: false);
        }

        await AddPhotosAsync(c, CaseImageKind.Report, photos, cancellationToken);
        c.History.Add(new CaseStatusHistory
        {
            NewStatus = c.Status,
            ChangedById = currentUser.Id,
            ChangedAt = now,
            Reason = category.IsEmergency ? "Emergency service type: approved automatically, handled by F4." : null,
        });

        db.Cases.Add(c);
        await db.SaveChangesAsync(cancellationToken);

        // The code exists only after the first save.
        var where = $"{room.Floor.DisplayName}, {room.RoomNumber}";
        if (category.IsEmergency)
        {
            await NotifyNewEmergencyAsync(c, room.Floor.BuildingId, where, cancellationToken);
        }
        else
        {
            // 5.4.2: every staff member of the building (N11).
            notifications.Send(
                await StaffOfBuildingAsync(room.Floor.BuildingId, cancellationToken),
                $"New request {Case.CodeOf(c.Id)} \"{c.Title}\" in {where}.");
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(c.Id, cancellationToken);
    }

    // ----- status changes (one method per arrow of the state machine) ---------------------------------------------

    public Task<CaseDetailResponse> CancelAsync(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Cancel, (c, now) =>
        {
            StopThirdParties(c, now);
            return Task.FromResult<string?>(request.Reason.Trim());
        }, cancellationToken);

    public Task<CaseDetailResponse> ApproveAsync(long id, ApproveCaseRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Approve, async (c, now) =>
        {
            var priority = await db.PriorityLevels.SingleAsync(p => p.Code == request.Urgency, cancellationToken);
            c.PriorityLevelId = priority.Id;
            c.Priority = priority;
            c.CostBearer = request.CostBearer;
            c.StaffId ??= currentUser.Id;
            if (request.Urgency != CasePriority.Emergency)
            {
                return null;
            }

            // F3 page 1: the staff member decides it is an emergency → F4, already acknowledged (it is their decision).
            EnterEmergency(c, priority, now, acknowledged: true);
            return "Escalated to Emergency, handed over to F4.";
        }, cancellationToken);

    public Task<CaseDetailResponse> RejectAsync(long id, RejectCaseRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Reject, (c, _) =>
        {
            c.StaffId ??= currentUser.Id;
            return Task.FromResult<string?>($"{RejectLabels[request.Reason]}: {request.Note.Trim()}");
        }, cancellationToken);

    public Task<CaseDetailResponse> AssignAsync(long id, AssignCaseRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Assign, async (c, _) =>
        {
            var technician = await db.Technicians.SingleOrDefaultAsync(
                t => t.Id == request.TechnicianId && t.Status == UserStatus.Active, cancellationToken)
                ?? throw CaseErrors.InvalidTechnician();

            c.AssignedTechnicianId = technician.Id;
            c.AcceptRemindedAt = null;
            c.Technician = technician;
            // The ERD keeps only the last technician on the case, so the history row says who got it.
            return $"Assigned to {technician.FullName}.";
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
            // Back to the staff member (status Approved) to assign someone else.
            c.AssignedTechnicianId = null;
            c.Technician = null;
            return Task.FromResult<string?>(request.Reason.Trim());
        }, cancellationToken);

    public Task<CaseDetailResponse> StartAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Start, (c, now) =>
        {
            // After a reassignment the clock keeps running from the first start.
            c.StartedAt ??= now;
            c.DueAt ??= c.StartedAt.Value.AddHours(c.Priority.SlaHours);
            return Task.FromResult<string?>(null);
        }, cancellationToken);

    public Task<CaseDetailResponse> HoldAsync(long id, HoldCaseRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Hold, (c, now) =>
        {
            c.HoldReason = request.Reason;
            c.HoldUntil = request.ExpectedResumeOn;
            if (request.Reason == HoldReason.WaitingThirdParty)
            {
                // F5 hand-over: all the system keeps is "this case has an outside party" (E4, E6).
                c.OutsourcedAt ??= now;
            }

            return Task.FromResult<string?>($"{HoldLabels[request.Reason]}, expected back {request.ExpectedResumeOn:yyyy-MM-dd}.");
        }, cancellationToken);

    public Task<CaseDetailResponse> ResumeAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Resume, (c, now) =>
        {
            var decided = c.HoldReason is HoldReason.WaitingStaffDecision or HoldReason.WaitingThirdParty;
            c.HoldReason = null;
            c.HoldUntil = null;
            // F5 step ⑤ "the technician continues": the staff drop any outside party still on the case.
            var stopped = StopThirdParties(c, now);
            return Task.FromResult(decided && currentUser.Role == UserRole.Staff
                ? $"Decision: the technician continues the repair.{(stopped > 0 ? " Outside party stopped." : string.Empty)}"
                : null);
        }, cancellationToken);

    public Task<CaseDetailResponse> CompleteAsync(long id, CompleteWorkRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Complete, async (c, now) =>
        {
            c.RootCause = request.RootCause.Trim();
            c.PartsUsed = string.IsNullOrWhiteSpace(request.PartsUsed) ? null : request.PartsUsed.Trim();
            c.CompletedAt = now;
            // Hours typed by the technician win; otherwise the system measures start to finish.
            c.ActualCompletionHours = request.LaborHours ?? MeasuredHours(c.StartedAt, now);
            // F5 step ⑨: an outside party replaced the asset → the new model goes to the replacement request.
            if (c.ThirdParties.Any(t => t.Status == OutsideWorkStatus.Done && t.Job == OutsideWorkJob.Replace))
            {
                if (string.IsNullOrWhiteSpace(request.ReplacementModel))
                {
                    throw CaseErrors.ReplacementModelRequired();
                }

                c.ReplacementModel = request.ReplacementModel.Trim();
                c.ReplacementSerial = string.IsNullOrWhiteSpace(request.ReplacementSerial) ? null : request.ReplacementSerial.Trim();
            }

            await AddPhotosAsync(c, CaseImageKind.Before, request.BeforePhotos ?? [], cancellationToken);
            await AddPhotosAsync(c, CaseImageKind.After, request.AfterPhotos, cancellationToken);
            AddCosts(c, request.Parts);
            return null;
        }, cancellationToken);

    // ----- F2: preventive work orders ---------------------------------------------------------------------------

    public Task<CaseDetailResponse> UpdateTaskAsync(
        long id, long taskId, UpdateTaskRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.UpdateTask, async (c, now) =>
        {
            var task = await db.CaseTasks.Include(t => t.Images).SingleOrDefaultAsync(t => t.Id == taskId && t.CaseId == c.Id, cancellationToken)
                ?? throw CaseErrors.TaskNotFound();
            var photos = request.Photos ?? [];
            if (request.Status == CaseTaskStatus.Fail && photos.Count == 0 && task.Images.Count == 0)
            {
                // TEC-05: a failed item needs a photo.
                throw CaseErrors.PhotoRequired(task.Name);
            }

            task.Status = request.Status;
            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                task.Notes.Add(new TaskNote { Note = request.Note.Trim() });
            }

            foreach (var photo in photos)
            {
                var path = await storage.SaveAsync("tasks", DataUrl.ParseImage(photo), cancellationToken);
                task.Images.Add(new TaskImage { Image = path });
            }

            if (request.Status != CaseTaskStatus.Fail || request.FailAction != FailAction.CreateCase || task.RepairCaseId is not null)
            {
                return null;
            }

            // BR-07: a repair case from the failed item — F3 at Approved for the staff to assign, F4 when dangerous.
            var urgency = request.Dangerous ? CasePriority.Emergency : CasePriority.Normal;
            var priority = await db.PriorityLevels.SingleAsync(p => p.Code == urgency, cancellationToken);
            var repair = new Case
            {
                RoomId = c.RoomId,
                AssetId = c.AssetId,
                CategoryId = c.CategoryId,
                Title = $"Repair: {task.Name}",
                Description = $"{request.Note?.Trim()}\nFound during preventive work order {Case.CodeOf(c.Id)}.",
                Status = CaseStatus.Approved,
                PriorityLevelId = priority.Id,
                CostBearer = CostBearer.Building,
                StaffId = c.StaffId,
                SubmittedAt = now,
            };
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
                Reason = $"Failed checklist item \"{task.Name}\" of {Case.CodeOf(c.Id)}.",
            });
            db.Cases.Add(repair);
            await db.SaveChangesAsync(cancellationToken);
            task.RepairCaseId = repair.Id;

            var kind = request.Dangerous ? "Emergency repair" : "Repair";
            notifications.Send(
                await StaffOfBuildingAsync(c.Room.Floor.BuildingId, cancellationToken),
                $"{kind} case {Case.CodeOf(repair.Id)} \"{repair.Title}\" was made from {Case.CodeOf(c.Id)}. Assign a technician.");
            return $"Item \"{task.Name}\" failed: repair case {Case.CodeOf(repair.Id)} made.";
        }, cancellationToken);

    public Task<CaseDetailResponse> CompleteChecklistAsync(long id, CompleteChecklistRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.CompleteChecklist, (c, now) =>
        {
            c.RootCause = string.IsNullOrWhiteSpace(request.Note) ? "Preventive maintenance done." : request.Note.Trim();
            c.CompletedAt = now;
            c.ActualCompletionHours = request.LaborHours ?? MeasuredHours(c.StartedAt, now);
            AddCosts(c, request.Parts);
            return Task.FromResult<string?>(null);
        }, cancellationToken);

    /// <summary>ERD: Costs — the parts the technician bought + one labour line (hours × hourly rate, E9).</summary>
    private static void AddCosts(Case c, IReadOnlyList<CostLineRequest>? parts)
    {
        c.Costs.AddRange((parts ?? []).Select(ToCost));
        if (c.Technician?.HourlyRate is { } rate && rate > 0 && c.ActualCompletionHours is { } hours)
        {
            c.Costs.Add(new Cost
            {
                Name = "Labour",
                Description = $"{c.Technician.FullName}: {hours:0.##} h × {rate:N0} VND",
                Amount = Math.Round(hours * rate, 0),
            });
        }
    }

    public Task<CaseDetailResponse> CloseAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Close, async (c, now) =>
        {
            c.ClosedAt = now;
            await AddReplacementRequestAsync(c, cancellationToken);
            return null;
        }, cancellationToken);

    public Task<CaseDetailResponse> ReopenAsync(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Reopen, (c, now) =>
        {
            ClearForReopen(c, now);
            return Task.FromResult<string?>(request.Reason.Trim());
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

    private static Cost ToCost(CostLineRequest line) =>
        new() { Name = line.Name.Trim(), Description = string.IsNullOrWhiteSpace(line.Description) ? null : line.Description.Trim(), Amount = line.Amount };

    // ----- Part 4: staff steps in, "not fixed", costs ------------------------------------------------------------

    public Task<CaseDetailResponse> ReassignAsync(long id, ReassignCaseRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Reassign, async (c, _) =>
        {
            var technician = await db.Technicians.SingleOrDefaultAsync(
                t => t.Id == request.TechnicianId && t.Status == UserStatus.Active, cancellationToken)
                ?? throw CaseErrors.InvalidTechnician();
            if (technician.Id == c.AssignedTechnicianId)
            {
                throw CaseErrors.SameTechnician();
            }

            var previous = c.Technician?.FullName ?? "nobody";
            c.AssignedTechnicianId = technician.Id;
            c.AcceptRemindedAt = null;
            c.Technician = technician;
            // The SLA clock keeps running: StartedAt / DueAt stay (Start does not reset them).
            return $"Reassigned from {previous} to {technician.FullName}: {request.Reason.Trim()}";
        }, cancellationToken);

    public Task<CaseDetailResponse> ChangeUrgencyAsync(long id, ChangeUrgencyRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.ChangeUrgency, async (c, now) =>
        {
            if (c.Priority.Code == request.Urgency)
            {
                throw CaseErrors.SameUrgency();
            }

            var before = c.Priority.Name;
            var priority = await db.PriorityLevels.SingleAsync(p => p.Code == request.Urgency, cancellationToken);
            c.PriorityLevelId = priority.Id;
            c.Priority = priority;
            if (request.Urgency == CasePriority.Emergency)
            {
                // F4 counts 24/7 from now, not from the start of work.
                EnterEmergency(c, priority, now, acknowledged: true);
            }
            else if (c.StartedAt is { } started)
            {
                // O08: the deadline follows the new level, counted from the start of work.
                c.DueAt = started.AddHours(priority.SlaHours);
                c.SlaWarnedAt = null;
                c.SlaOverdueNotifiedAt = null;
            }

            var f4 = request.Urgency == CasePriority.Emergency ? " Handed over to F4." : string.Empty;
            return $"Urgency {before} → {priority.Name}: {request.Reason.Trim()}{f4}";
        }, cancellationToken);

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
                $"Deadline moved to {request.NewDueAt:yyyy-MM-dd HH:mm} UTC: {ExtendLabels[request.Reason]}.{note}");
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

    public Task<CaseDetailResponse> DeclineNotFixedAsync(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.DeclineNotFixed, (c, _) =>
        {
            c.NotFixedReason = null;
            c.NotFixedAt = null;
            return Task.FromResult<string?>($"Not-fixed report declined: {request.Reason.Trim()}");
        }, cancellationToken);

    public Task<CaseDetailResponse> UpdateCostsAsync(long id, UpdateCostsRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.UpdateCosts, async (c, _) =>
        {
            db.Costs.RemoveRange(await db.Costs.Where(x => x.CaseId == c.Id).ToListAsync(cancellationToken));
            c.Costs.AddRange(request.Lines.Select(ToCost));
            return $"Costs corrected, total {request.Lines.Sum(l => l.Amount):N0} VND.";
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
    /// (which changes the fields and returns the reason for the history, if any), moves the status, writes one history row.
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

        // Read before apply: a decline clears the technician, a passed outside-work check moves the status itself.
        var technicianBefore = c.AssignedTechnicianId;
        var rule = CaseTransitions.Find(action, scope.Role, c.Status)!;
        var reason = await apply(c, now);

        // A status change always writes a history row; an action that keeps the status (urgency, deadline, "not fixed",
        // costs) writes one with the same status when it has a reason, so the timeline shows who did it and why.
        var to = rule.To;
        if (to is not null || reason is not null)
        {
            c.Status = to ?? c.Status;
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

    /// <summary>Who hears about each arrow. The person who acted is never notified (NotificationService skips them).</summary>
    private async Task NotifyAsync(
        Case c, CaseAction action, string? reason, long? technicianBefore, CancellationToken cancellationToken)
    {
        var code = Case.CodeOf(c.Id);
        var customer = c.RequestingCustomerId is { } customerId ? new[] { customerId } : [];
        var technician = c.AssignedTechnicianId is { } technicianId ? new[] { technicianId } : [];
        var staffOfBuilding = await StaffOfBuildingAsync(c.Room.Floor.BuildingId, cancellationToken);

        switch (action)
        {
            case CaseAction.Approve:
                notifications.Send(customer, $"{code} \"{c.Title}\" was approved and will be assigned to a technician.");
                break;
            case CaseAction.Reject:
                notifications.Send(customer, $"{code} \"{c.Title}\" was rejected. {reason}");
                break;
            case CaseAction.Assign:
                notifications.Send(technician, CaseMapper.IsEmergencyFlow(c)
                    ? $"EMERGENCY {code} \"{c.Title}\" in {c.Room.Floor.DisplayName}, {c.Room.RoomNumber} was assigned to you. Accept it now."
                    : $"{code} \"{c.Title}\" was assigned to you.");
                break;
            case CaseAction.Accept:
                // 5.4.2: the staff member who assigned it (the last "assigned" row of the history).
                var assignedBy = await db.CaseStatusHistory
                    .Where(h => h.CaseId == c.Id && h.NewStatus == CaseStatus.Assigned)
                    .OrderByDescending(h => h.ChangedAt).Select(h => (long?)h.ChangedById)
                    .FirstOrDefaultAsync(cancellationToken);
                notifications.Send(assignedBy is { } by ? [by] : [], $"{c.Technician?.FullName} accepted {code}.");
                break;
            case CaseAction.Decline:
                notifications.Send(staffOfBuilding, $"A technician declined {code}: {reason} Assign someone else.");
                break;
            case CaseAction.Hold:
                notifications.Send([.. staffOfBuilding, .. customer], $"{code} \"{c.Title}\" is on hold. {reason}");
                break;
            case CaseAction.Complete or CaseAction.CompleteChecklist:
                notifications.Send(staffOfBuilding, $"{code} \"{c.Title}\" is done and waits for your check.");
                break;
            case CaseAction.Close:
                notifications.Send(customer, $"{code} \"{c.Title}\" was closed. Please rate the repair.");
                await NotifyEmergencyClosedAsync(c, staffOfBuilding, cancellationToken);
                break;
            case CaseAction.Reopen or CaseAction.AcceptNotFixed:
                notifications.Send([.. technician, .. customer], $"{code} \"{c.Title}\" was reopened: {reason}");
                break;
            case CaseAction.Reassign:
                notifications.Send(technicianBefore is { } old ? [old] : [], $"{code} \"{c.Title}\" was taken back from you and given to another technician.");
                notifications.Send(technician, $"{code} \"{c.Title}\" was assigned to you.");
                break;
            case CaseAction.ChangeUrgency or CaseAction.ExtendDue:
                notifications.Send([.. technician, .. customer], $"{code} \"{c.Title}\": {reason}");
                break;
            case CaseAction.ReportNotFixed:
                notifications.Send(staffOfBuilding, $"{code} \"{c.Title}\": the customer says it is not fixed. Reopen it or answer.");
                break;
            case CaseAction.DeclineNotFixed:
                notifications.Send(customer, $"{code} \"{c.Title}\": {reason}");
                break;
            case CaseAction.Cancel:
                long[] involved = [.. customer, .. (c.StaffId is { } staffId ? new[] { staffId } : []), .. (technicianBefore is { } t ? new[] { t } : [])];
                notifications.Send(involved, $"{code} \"{c.Title}\" was cancelled: {reason}");
                break;
            default:
                NotifyEmergency(c, action, reason, technicianBefore, staffOfBuilding);
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

        return action switch
        {
            CaseAction.Rate => c.RatingStars is null,
            // F2: a preventive work order is done through its checklist, an incident through the repair record.
            CaseAction.Complete => !c.IsPreventive,
            CaseAction.UpdateTask => c.IsPreventive,
            CaseAction.CompleteChecklist => c.IsPreventive && c.Tasks.All(t => t.Status != CaseTaskStatus.Pending),
            // O09: only before the deadline passes.
            CaseAction.ExtendDue => c.DueAt is { } due && now < due,
            CaseAction.ReportNotFixed => c.NotFixedAt is null && c.ClosedAt is { } closedAt && now - closedAt <= ReopenWindow,
            CaseAction.AcceptNotFixed or CaseAction.DeclineNotFixed => c.NotFixedAt is not null,
            CaseAction.Reopen when c.Status == CaseStatus.Closed => c.ClosedAt is { } closed && now - closed <= ReopenWindow,
            // F5: while the case waits for the staff's decision or an outside party, the technician does not resume it.
            CaseAction.Resume when scope.Role == UserRole.Technician =>
                c.HoldReason is not (HoldReason.WaitingStaffDecision or HoldReason.WaitingThirdParty),
            CaseAction.RequestDecision => !c.IsPreventive,
            CaseAction.Outsource => !c.IsPreventive && !c.ThirdParties.Any(t => t.Status is
                OutsideWorkStatus.Quoting or OutsideWorkStatus.AwaitingDecision or OutsideWorkStatus.Working),
            CaseAction.RecordQuote => ActiveThirdParty(c)?.Status == OutsideWorkStatus.Quoting,
            CaseAction.DecideQuote => ActiveThirdParty(c)?.Status == OutsideWorkStatus.AwaitingDecision,
            CaseAction.OutsideDeclined => ActiveThirdParty(c)?.Status is OutsideWorkStatus.Quoting or OutsideWorkStatus.Working,
            CaseAction.CheckOutsideWork => ActiveThirdParty(c)?.Status == OutsideWorkStatus.Working,
            CaseAction.RedoOutsideWork => ActiveThirdParty(c)?.Status == OutsideWorkStatus.FailedCheck,
            // F4: an emergency leaves its level only through "downgrade" (K04), which needs a cost bearer.
            CaseAction.ChangeUrgency => !emergency,
            CaseAction.Acknowledge => emergency && c.AcknowledgedAt is null,
            CaseAction.Downgrade or CaseAction.Merge or CaseAction.AddLog => emergency,
            CaseAction.MarkSafe => emergency && c.MadeSafeAt is null,
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
            UserRole.Customer => query.Where(c => c.RequestingCustomerId == userId),
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
        .Include(c => c.Priority)
        .Include(c => c.Customer)
        .Include(c => c.Staff)
        .Include(c => c.Technician)
        .Include(c => c.TriggerWorkOrder)
        .Include(c => c.Tasks)
        .Include(c => c.ThirdParties);

    private IQueryable<Case> WithDetail() => WithRefs()
        .Include(c => c.Asset)
        .Include(c => c.Images)
        .Include(c => c.History).ThenInclude(h => h.ChangedBy)
        .Include(c => c.Rejections).ThenInclude(r => r.Technician)
        .Include(c => c.Costs)
        .Include(c => c.Tasks).ThenInclude(t => t.Notes)
        .Include(c => c.Tasks).ThenInclude(t => t.Images)
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

    private static decimal MeasuredHours(DateTimeOffset? started, DateTimeOffset now) =>
        started is null ? 0.01m : Math.Max(0.01m, Math.Round((decimal)(now - started.Value).TotalHours, 2));
}
