using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Storage;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>
/// F5 repair or replace (docs/flows/F5_Sua_Hay_Thay, Leader 02/11). The technician asks for the staff's decision with an
/// assessment; the staff decides. Meeting 07/11: the outside party is agreed outside the system: the staff only presses
/// "Hand to an outside party" (CaseService.WorkOrder.cs) and marks the tasks it does. The old outside-party rows
/// (case_third_parties) are only shown; no quote, refusal or check is recorded any more.
/// </summary>
internal sealed partial class CaseService
{
    /// <summary>The case is cancelled: every unfinished outside party of the old flow is cancelled.</summary>
    private static int StopThirdParties(Case c, DateTimeOffset now)
    {
        var active = c.ThirdParties.Where(t => t.IsActive).ToList();
        foreach (var t in active)
        {
            t.Status = OutsideWorkStatus.Cancelled;
            t.FinishedAt = now;
        }

        return active.Count;
    }

    // ----- technician -------------------------------------------------------------------------------------------

    /// <summary>
    /// Meeting 07/11 (replaces the assessment): the technician asks the staff to hand the job to an outside party —
    /// description, reason and photos (kept in the assessment columns and the "assessment" photos). On Hold, clock stopped.
    /// </summary>
    public Task<CaseDetailResponse> RequestDecisionAsync(long id, RequestDecisionRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.RequestDecision, async (c, now) =>
        {
            StopClock(c, now);
            c.AssessmentCondition = request.Description.Trim();
            c.AssessmentCause = request.Reason.Trim();
            c.AssessmentRecommendation = null;
            c.AssessedAt = now;
            c.HoldReason = HoldReason.WaitingStaffDecision;
            c.HoldUntil = null;
            await AddPhotosAsync(c, CaseImageKind.Assessment, request.Photos, cancellationToken);

            notifications.Send(
                c.StaffId is { } s ? [s] : await StaffOfBuildingAsync(c.Room.Floor.BuildingId, cancellationToken),
                $"{c.Technician?.FullName} asks to hand {Case.CodeOf(c.Id)} \"{c.Title}\" to an outside party: {c.AssessmentCause}");
            return $"{CaseHistoryTags.NeedsDecision}: {c.AssessmentCause}";
        }, cancellationToken);

    /// <summary>F5 way 2: a survey case for an asset of the caller's buildings (no customer, Approved, not in the KPI).</summary>
    public async Task<CaseDetailResponse> OpenSurveyAsync(OpenSurveyRequest request, CancellationToken cancellationToken)
    {
        var scope = await LoadScopeAsync(cancellationToken);
        var asset = await db.Assets.Include(a => a.Room).ThenInclude(r => r.Floor)
            .SingleOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);
        var open = await db.Cases.AnyAsync(c => c.AssetId == request.AssetId && c.IsSurvey
            && c.Status != CaseStatus.Closed && c.Status != CaseStatus.Cancelled, cancellationToken);
        if (asset is null || asset.IsRetired || open || !scope.BuildingIds.Contains(asset.Room.Floor.BuildingId))
        {
            throw CaseErrors.SurveyNotAllowed();
        }

        var category = await db.CaseCategories.Include(x => x.DefaultPriority)
            .SingleOrDefaultAsync(x => x.Id == request.ServiceTypeId, cancellationToken)
            ?? throw CaseErrors.UnknownServiceType();

        var now = clock.GetUtcNow();
        var repairs = await AssetRepairStats.RepairsSinceAsync(db, asset.Id, now - AssetRepairStats.Window, cancellationToken);
        var c = new Case
        {
            RoomId = asset.RoomId,
            AssetId = asset.Id,
            CategoryId = category.Id,
            Title = $"Survey: {asset.Name}",
            Description = string.IsNullOrWhiteSpace(request.Note)
                ? $"Survey of an asset that failed {repairs} times in 12 months: repair or replace?"
                : request.Note.Trim(),
            Status = CaseStatus.Approved,
            PriorityLevelId = category.DefaultPriority.Id,
            StaffId = currentUser.Id,
            IsSurvey = true,
            SubmittedAt = now,
        };
        c.Tasks.Add(new Entities.Maintenance.CaseTask { Name = c.Title, Status = Entities.Maintenance.CaseTaskStatus.Pending });
        c.History.Add(new CaseStatusHistory
        {
            NewStatus = CaseStatus.Approved,
            ChangedById = currentUser.Id,
            ChangedAt = now,
            Reason = $"Asset survey opened: {repairs} repairs in 12 months.",
        });
        db.Cases.Add(c);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(c.Id, cancellationToken);
    }

    // ----- decision board, replacement --------------------------------------------------------------------------

    /// <summary>Step ③ facts for the staff (G4, G5). Only for a case on an asset that went to F5 (assessed, survey, outside party).</summary>
    private async Task<DecisionInfoResponse?> DecisionInfoAsync(Case c, CancellationToken cancellationToken)
    {
        if (c.AssetId is not { } assetId || (c.AssessedAt is null && !c.IsSurvey && c.ThirdParties.Count == 0))
        {
            return null;
        }

        var asset = await db.Assets.AsNoTracking().Include(a => a.Type).Include(a => a.Manufacturer)
            .SingleAsync(a => a.Id == assetId, cancellationToken);
        var now = clock.GetUtcNow();
        var since = now - AssetRepairStats.Window;
        var repairs = await AssetRepairStats.RepairsSinceAsync(db, assetId, since, cancellationToken);
        var reopens = await db.CaseStatusHistory.AsNoTracking()
            .CountAsync(h => h.Case.AssetId == assetId && h.NewStatus == CaseStatus.Reopened && h.ChangedAt >= since, cancellationToken);
        // The ERD has no install date: the age counts from when the asset was registered.
        var age = Math.Round((decimal)(now - asset.CreatedAt).TotalDays / 365.25m, 1);
        var today = DateOnly.FromDateTime(now.ToOffset(TimeSpan.FromHours(7)).DateTime);
        var underWarranty = asset.WarrantyEndDate >= today;
        var life = asset.Type.ExpectedLifeYears;

        var reasons = new List<string>();
        if (underWarranty)
        {
            reasons.Add($"Under warranty until {asset.WarrantyEndDate:yyyy-MM-dd}: send it to the manufacturer first.");
        }

        if (asset.HasCopyright)
        {
            reasons.Add("Only the manufacturer may service this asset.");
        }

        if (repairs >= AssetRepairStats.FrequentFailureThreshold)
        {
            reasons.Add($"{repairs} repairs in the last 12 months.");
        }

        if (c.AssessmentRecommendation == TechnicianRecommendation.Replace)
        {
            reasons.Add("The technician recommends replacing it.");
        }

        if (life is { } years && age >= years)
        {
            reasons.Add($"Past its normal life ({age} of {years} years).");
        }

        var suggest = !underWarranty && (repairs >= AssetRepairStats.FrequentFailureThreshold
            || c.AssessmentRecommendation == TechnicianRecommendation.Replace || (life is { } l && age >= l));
        return new DecisionInfoResponse(
            repairs, reopens, age, life, asset.WarrantyEndDate, underWarranty, asset.HasCopyright,
            asset.Manufacturer?.Name, asset.Manufacturer?.PhoneNumber, c.AssessmentRecommendation, suggest, reasons);
    }

    /// <summary>
    /// A06: closing a case whose asset an outside party replaced asks the Administrator to register the new asset and
    /// retire the old one. The Administrator gets the asset, the model and the case code, not the case content (BR-11).
    /// </summary>
    private async Task AddReplacementRequestAsync(Case c, CancellationToken cancellationToken)
    {
        if (c.AssetId is not { } assetId || c.ReplacementModel is null
            || await db.AssetReplacementRequests.AnyAsync(r => r.CaseId == c.Id, cancellationToken))
        {
            return;
        }

        db.AssetReplacementRequests.Add(new AssetReplacementRequest
        {
            CaseId = c.Id,
            OldAssetId = assetId,
            NewModel = c.ReplacementModel,
            NewSerial = c.ReplacementSerial,
            RequestedById = currentUser.Id,
            Status = ReplacementStatus.Open,
        });
        var assetName = await db.Assets.Where(a => a.Id == assetId).Select(a => a.Name).SingleAsync(cancellationToken);
        var admins = await db.Admins.Where(a => a.Status == UserStatus.Active).Select(a => a.Id).ToListAsync(cancellationToken);
        notifications.Send(admins,
            $"Asset replaced: register \"{c.ReplacementModel}\" in place of \"{assetName}\" ({c.Room.Floor.Building.Name}, {c.Room.RoomNumber}).");
    }
}
