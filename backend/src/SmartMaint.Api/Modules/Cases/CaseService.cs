using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Users;
using SmartMaint.Api.Modules.Cases.Dtos;

namespace SmartMaint.Api.Modules.Cases;

internal sealed class CaseService(AppDbContext db, ICurrentUser currentUser, TimeProvider clock) : ICaseService
{
    /// <summary>The customer may report "not fixed" / the staff member may reopen a closed case for this long (F3 step 12).</summary>
    private static readonly TimeSpan ReopenWindow = TimeSpan.FromDays(7);

    private static readonly Dictionary<RejectReason, string> RejectLabels = new()
    {
        [RejectReason.MissingInfo] = "Missing information",
        [RejectReason.OutOfScope] = "Outside the building's service scope",
        [RejectReason.Duplicate] = "A case for this problem already exists",
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
        var query = Visible(WithRefs().AsNoTracking(), scope);

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

        return CaseMapper.ToDetail(c, AllowedActions(c, scope, clock.GetUtcNow()));
    }

    // ----- create -----------------------------------------------------------------------------------------------

    public async Task<CaseDetailResponse> CreateAsync(CreateCaseRequest request, CancellationToken cancellationToken)
    {
        var category = await db.CaseCategories.Include(x => x.DefaultPriority)
            .SingleOrDefaultAsync(x => x.Id == request.ServiceTypeId, cancellationToken)
            ?? throw CaseErrors.UnknownServiceType();

        if (!await db.Rooms.AnyAsync(r => r.Id == request.RoomId, cancellationToken))
        {
            throw CaseErrors.UnknownRoom();
        }

        if (request.AssetId is { } assetId
            && !await db.Assets.AnyAsync(a => a.Id == assetId && a.RoomId == request.RoomId, cancellationToken))
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
            RoomId = request.RoomId,
            AssetId = request.AssetId,
            CategoryId = category.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = CaseStatus.Pending,
            PriorityLevelId = priority.Id,
            CostBearer = category.DefaultCostBearer,
            SubmittedAt = now,
        };
        c.Images.AddRange(photos.Select(p => new CaseImage { Kind = CaseImageKind.Report, Content = p }));
        c.History.Add(new CaseStatusChange
        {
            FromStatus = null,
            ToStatus = CaseStatus.Pending,
            ChangedById = currentUser.Id,
            ChangedAt = now,
            Reason = category.IsEmergency ? "Emergency service type, handed over to F4." : null,
        });

        db.Cases.Add(c);
        await db.SaveChangesAsync(cancellationToken);
        // TODO(notification): tell the staff of the building (all of them, N11) that a case is waiting.
        return await GetAsync(c.Id, cancellationToken);
    }

    // ----- status changes (one method per arrow of the state machine) ---------------------------------------------

    public Task<CaseDetailResponse> CancelAsync(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Cancel, (_, _) => Task.FromResult<string?>(request.Reason.Trim()), cancellationToken);

    public Task<CaseDetailResponse> ApproveAsync(long id, ApproveCaseRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Approve, async (c, _) =>
        {
            var priority = await db.PriorityLevels.SingleAsync(p => p.Code == request.Urgency, cancellationToken);
            c.PriorityLevelId = priority.Id;
            c.Priority = priority;
            c.CostBearer = request.CostBearer;
            c.StaffId ??= currentUser.Id;
            return request.Urgency == CasePriority.Emergency ? "Escalated to Emergency, handed over to F4." : null;
        }, cancellationToken);

    public Task<CaseDetailResponse> RejectAsync(long id, RejectCaseRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Reject, (c, _) =>
        {
            c.StaffId ??= currentUser.Id;
            return Task.FromResult<string?>($"{RejectLabels[request.Reason]}: {request.Note.Trim()}");
        }, cancellationToken);

    public Task<CaseDetailResponse> AssignAsync(long id, AssignCaseRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Assign, async (c, now) =>
        {
            var technician = await db.Users.SingleOrDefaultAsync(
                u => u.Id == request.TechnicianId && u.Role == UserRole.Technician && u.Status == UserStatus.Active,
                cancellationToken) ?? throw CaseErrors.InvalidTechnician();

            c.Assignments.Add(new CaseAssignment
            {
                TechnicianId = technician.Id,
                AssignedById = currentUser.Id,
                AssignedAt = now,
                Response = AssignmentAnswer.Waiting,
            });
            c.AssignedTechnicianId = technician.Id;
            c.Technician = technician;
            // TODO(notification): tell the technician.
            return null;
        }, cancellationToken);

    public Task<CaseDetailResponse> AcceptAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Accept, (c, now) =>
        {
            var assignment = WaitingAssignment(c);
            assignment.Response = AssignmentAnswer.Accepted;
            assignment.RespondedAt = now;
            return Task.FromResult<string?>(null);
        }, cancellationToken);

    public Task<CaseDetailResponse> DeclineAsync(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Decline, (c, now) =>
        {
            var assignment = WaitingAssignment(c);
            assignment.Response = AssignmentAnswer.Declined;
            assignment.RespondedAt = now;
            assignment.DeclineReason = request.Reason.Trim();
            // Back to the staff member (status Approved) to assign someone else. The manager counts these declines (N10).
            c.AssignedTechnicianId = null;
            c.Technician = null;
            return Task.FromResult<string?>(request.Reason.Trim());
        }, cancellationToken);

    public Task<CaseDetailResponse> StartAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Start, (c, now) =>
        {
            c.StartedAt = now;
            c.DueAt = now.AddHours(c.Priority.SlaHours);
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
        ActAsync(id, CaseAction.Resume, (c, _) =>
        {
            c.HoldReason = null;
            c.HoldUntil = null;
            return Task.FromResult<string?>(null);
        }, cancellationToken);

    public Task<CaseDetailResponse> CompleteAsync(long id, CompleteWorkRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Complete, (c, now) =>
        {
            c.RootCause = request.RootCause.Trim();
            c.PartsUsed = string.IsNullOrWhiteSpace(request.PartsUsed) ? null : request.PartsUsed.Trim();
            c.CompletedAt = now;
            // Hours typed by the technician win; otherwise the system measures start to finish.
            c.ActualHours = request.LaborHours ?? MeasuredHours(c.StartedAt, now);
            c.Images.AddRange((request.BeforePhotos ?? []).Select(p => new CaseImage { Kind = CaseImageKind.Before, Content = p }));
            c.Images.AddRange(request.AfterPhotos.Select(p => new CaseImage { Kind = CaseImageKind.After, Content = p }));
            // TODO(notification): tell the staff member there is work to verify.
            return Task.FromResult<string?>(null);
        }, cancellationToken);

    public Task<CaseDetailResponse> CloseAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Close, (c, now) =>
        {
            c.ClosedAt = now;
            // TODO(asset-history): write the repair into the asset's history (F1).
            return Task.FromResult<string?>(null);
        }, cancellationToken);

    public Task<CaseDetailResponse> ReopenAsync(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Reopen, (c, now) =>
        {
            // BR-04: by default the same technician gets it back and must accept again.
            if (c.AssignedTechnicianId is { } technicianId)
            {
                c.Assignments.Add(new CaseAssignment
                {
                    TechnicianId = technicianId,
                    AssignedById = currentUser.Id,
                    AssignedAt = now,
                    Response = AssignmentAnswer.Waiting,
                });
            }

            // The SLA starts over with the next "start"; the old times stay in the status history.
            c.StartedAt = null;
            c.DueAt = null;
            c.CompletedAt = null;
            c.ClosedAt = null;
            return Task.FromResult<string?>(request.Reason.Trim());
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
        var c = await Visible(WithRefs().Include(x => x.Assignments), scope)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw CaseErrors.NotFound();

        var now = clock.GetUtcNow();
        if (!IsAllowed(c, action, scope, now))
        {
            throw CaseErrors.InvalidTransition();
        }

        var from = c.Status;
        var reason = await apply(c, now);

        var to = CaseTransitions.Find(action, scope.Role, from)!.To;
        if (to is { } next)
        {
            c.Status = next;
            c.History.Add(new CaseStatusChange
            {
                FromStatus = from,
                ToStatus = next,
                ChangedById = scope.UserId,
                ChangedAt = now,
                Reason = reason,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    // ----- rules ------------------------------------------------------------------------------------------------

    /// <summary>State machine row + emergency rule + "is this case mine to act on" (B3) + the extra conditions of a few actions.</summary>
    private static bool IsAllowed(Case c, CaseAction action, Scope scope, DateTimeOffset now)
    {
        if (CaseTransitions.Find(action, scope.Role, c.Status) is null)
        {
            return false;
        }

        // Emergency cases belong to F4 (Tùng); F3 offers no action on them.
        if (CaseMapper.IsEmergencyFlow(c))
        {
            return false;
        }

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
            CaseAction.Reopen when c.Status == CaseStatus.Closed => c.ClosedAt is { } closed && now - closed <= ReopenWindow,
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
            UserRole.Technician => query.Where(c =>
                c.AssignedTechnicianId == userId || c.Assignments.Any(a => a.TechnicianId == userId)),
            _ => query.Where(_ => false),
        };
    }

    private IQueryable<Case> WithRefs() => db.Cases
        .Include(c => c.Room).ThenInclude(r => r.Floor).ThenInclude(f => f.Building)
        .Include(c => c.Category)
        .Include(c => c.Priority)
        .Include(c => c.Customer)
        .Include(c => c.Staff)
        .Include(c => c.Technician);

    private IQueryable<Case> WithDetail() => WithRefs()
        .Include(c => c.Asset)
        .Include(c => c.Images)
        .Include(c => c.History).ThenInclude(h => h.ChangedBy)
        .Include(c => c.Assignments).ThenInclude(a => a.Technician)
        .Include(c => c.Assignments).ThenInclude(a => a.AssignedBy)
        .AsSplitQuery();

    private CaseAssignment WaitingAssignment(Case c) =>
        c.Assignments
            .Where(a => a.TechnicianId == currentUser.Id && a.Response == AssignmentAnswer.Waiting)
            .OrderByDescending(a => a.Id)
            .FirstOrDefault()
        ?? throw CaseErrors.NoOpenAssignment();

    private static decimal MeasuredHours(DateTimeOffset? started, DateTimeOffset now) =>
        started is null ? 0.01m : Math.Max(0.01m, Math.Round((decimal)(now - started.Value).TotalHours, 2));
}
