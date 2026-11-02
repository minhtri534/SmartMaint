using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Storage;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>
/// F5 repair or replace (docs/flows/F5_Sua_Hay_Thay, Leader 02/11). The case waits On Hold: first for the staff's
/// decision (the technician's assessment), then for the outside party (manufacturer, package contract, or a quote the
/// payer accepts). No stock, no money, no manager approval (N2, N22, BR-12): the system records who, when and why.
/// </summary>
internal sealed partial class CaseService
{
    private static readonly Dictionary<TechnicianRecommendation, string> RecommendationLabels = new()
    {
        [TechnicianRecommendation.Repairable] = "repairable with more time or parts",
        [TechnicianRecommendation.Replace] = "replace the asset",
        [TechnicianRecommendation.Specialist] = "needs an outside specialist",
    };

    private static readonly Dictionary<OutsideWorkKind, string> KindLabels = new()
    {
        [OutsideWorkKind.Warranty] = "warranty",
        [OutsideWorkKind.Contract] = "maintenance contract",
        [OutsideWorkKind.Quote] = "quote first",
    };

    private const string EvidenceFolder = "quotes";

    /// <summary>The outside party the case is waiting on (latest one not finished).</summary>
    private static CaseThirdParty? ActiveThirdParty(Case c) =>
        c.ThirdParties.Where(t => t.IsActive).OrderByDescending(t => t.Id).FirstOrDefault();

    /// <summary>The staff chose another way (or cancelled the case): every unfinished outside party is cancelled.</summary>
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

    private static string JobText(OutsideWorkJob job) => job == OutsideWorkJob.Replace ? "replace the asset" : "repair it";

    private static string When(DateTimeOffset? at) => at is { } a ? $" on {a.ToOffset(TimeSpan.FromHours(7)):yyyy-MM-dd HH:mm}" : string.Empty;

    // ----- technician -------------------------------------------------------------------------------------------

    /// <summary>Steps ①–②: the assessment (condition, cause, recommendation, photos) and On Hold "waiting for the staff's decision".</summary>
    public Task<CaseDetailResponse> RequestDecisionAsync(long id, RequestDecisionRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.RequestDecision, async (c, now) =>
        {
            c.AssessmentCondition = request.Condition.Trim();
            c.AssessmentCause = request.Cause.Trim();
            c.AssessmentRecommendation = request.Recommendation;
            c.AssessedAt = now;
            c.HoldReason = HoldReason.WaitingStaffDecision;
            c.HoldUntil = null;
            await AddPhotosAsync(c, CaseImageKind.Assessment, request.Photos, cancellationToken);

            var label = RecommendationLabels[request.Recommendation];
            notifications.Send(
                c.StaffId is { } s ? [s] : await StaffOfBuildingAsync(c.Room.Floor.BuildingId, cancellationToken),
                $"{c.Technician?.FullName} needs your decision on {Case.CodeOf(c.Id)} \"{c.Title}\": {label}.");
            return $"Needs the staff's decision ({label}): {c.AssessmentCondition}";
        }, cancellationToken);

    /// <summary>Step ⑧: the technician, as contact person, checked the outside party's work. OK → back to In Progress.</summary>
    public Task<CaseDetailResponse> CheckOutsideWorkAsync(long id, CheckOutsideWorkRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.CheckOutsideWork, async (c, now) =>
        {
            var party = ActiveThirdParty(c)!;
            var note = string.IsNullOrWhiteSpace(request.Note) ? string.Empty : $": {request.Note.Trim()}";
            var staff = c.StaffId is { } s ? new List<long> { s } : await StaffOfBuildingAsync(c.Room.Floor.BuildingId, cancellationToken);
            if (!request.Passed)
            {
                party.Status = OutsideWorkStatus.FailedCheck;
                notifications.Send(staff,
                    $"{Case.CodeOf(c.Id)}: the work of {party.CompanyName} is not OK{note}. Ask them to redo it or choose another way.");
                return $"Work of {party.CompanyName} checked, not OK{note}";
            }

            party.Status = OutsideWorkStatus.Done;
            party.FinishedAt = now;
            c.Status = CaseStatus.InProgress;
            c.HoldReason = null;
            c.HoldUntil = null;
            notifications.Send(staff, $"{Case.CodeOf(c.Id)}: the work of {party.CompanyName} is OK, the technician finishes the case.");
            return $"Work of {party.CompanyName} checked OK{note}";
        }, cancellationToken);

    // ----- staff ------------------------------------------------------------------------------------------------

    /// <summary>Steps ④–⑤: the manufacturer (warranty), a contractor on a package contract, or a contractor who quotes first.</summary>
    public Task<CaseDetailResponse> OutsourceAsync(long id, OutsourceRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Outsource, (c, now) =>
        {
            // A failed check the staff do not redo: they chose another party.
            StopThirdParties(c, now);
            var party = new CaseThirdParty
            {
                Kind = request.Kind,
                Job = request.Job,
                Status = request.Kind == OutsideWorkKind.Quote ? OutsideWorkStatus.Quoting : OutsideWorkStatus.Working,
                CompanyName = request.CompanyName.Trim(),
                Contact = request.Contact.Trim(),
                // Postgres stores UTC only; the API accepts any offset.
                AppointmentAt = request.AppointmentAt?.ToUniversalTime(),
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            };
            c.ThirdParties.Add(party);
            c.HoldReason = HoldReason.WaitingThirdParty;
            c.HoldUntil = request.AppointmentAt is { } at ? DateOnly.FromDateTime(at.ToOffset(TimeSpan.FromHours(7)).DateTime) : null;
            c.OutsourcedAt ??= now;

            var code = Case.CodeOf(c.Id);
            var what = request.Kind == OutsideWorkKind.Quote
                ? $"{party.CompanyName} will survey and quote to {JobText(request.Job)}"
                : $"{party.CompanyName} will {JobText(request.Job)}{When(request.AppointmentAt)}";
            notifications.Send(c.AssignedTechnicianId is { } t ? [t] : [], $"{code} \"{c.Title}\": {what}. You are the contact person ({party.Contact}).");
            if (request.Kind != OutsideWorkKind.Quote)
            {
                notifications.Send(c.RequestingCustomerId is { } r ? [r] : [], $"{code} \"{c.Title}\": {what}.");
            }

            var note = party.Note is null ? string.Empty : $" {party.Note}";
            return Task.FromResult<string?>(
                $"Handed to {party.CompanyName} ({KindLabels[request.Kind]}, {JobText(request.Job)}){When(request.AppointmentAt)}.{note}");
        }, cancellationToken);

    /// <summary>The quote arrived: the building decides outside the system, or it goes to the customer's contract representative (R04).</summary>
    public Task<CaseDetailResponse> RecordQuoteAsync(long id, RecordQuoteRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.RecordQuote, (c, now) =>
        {
            var party = ActiveThirdParty(c)!;
            party.QuoteBearer = request.Bearer;
            party.Status = OutsideWorkStatus.AwaitingDecision;
            var note = string.IsNullOrWhiteSpace(request.Note) ? string.Empty : $" {request.Note.Trim()}";
            if (request.Bearer == CostBearer.Customer)
            {
                party.QuoteSentTo = request.SentTo!.Trim();
                party.QuoteSentAt = now;
                return Task.FromResult<string?>($"Quote of {party.CompanyName} sent to {party.QuoteSentTo} (the customer pays).{note}");
            }

            return Task.FromResult<string?>($"Quote of {party.CompanyName} received: the building decides.{note}");
        }, cancellationToken);

    /// <summary>Step ⑥: the payer's decision with its evidence. Go → the party works; no → the staff choose another way.</summary>
    public Task<CaseDetailResponse> DecideQuoteAsync(long id, QuoteDecisionRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.DecideQuote, async (c, now) =>
        {
            var party = ActiveThirdParty(c)!;
            party.DecisionGo = request.Go;
            party.DecidedBy = request.DecidedBy.Trim();
            party.DecidedAt = now;
            if (!string.IsNullOrWhiteSpace(request.Evidence))
            {
                party.EvidenceImage = await storage.SaveAsync(EvidenceFolder, DataUrl.ParseImage(request.Evidence), cancellationToken);
            }

            var note = string.IsNullOrWhiteSpace(request.Note) ? string.Empty : $" {request.Note.Trim()}";
            if (!request.Go)
            {
                party.Status = OutsideWorkStatus.Declined;
                party.FinishedAt = now;
                c.HoldReason = HoldReason.WaitingStaffDecision;
                return $"Quote of {party.CompanyName} declined by {party.DecidedBy}.{note}";
            }

            party.Status = OutsideWorkStatus.Working;
            c.CostBearer = party.QuoteBearer ?? c.CostBearer;
            notifications.Send(c.AssignedTechnicianId is { } t ? [t] : [],
                $"{Case.CodeOf(c.Id)} \"{c.Title}\": the quote was accepted, {party.CompanyName} will {JobText(party.Job)}.");
            return $"Quote of {party.CompanyName} accepted by {party.DecidedBy}.{note}";
        }, cancellationToken);

    /// <summary>The manufacturer refused the warranty claim (or the contractor will not do it): back to the staff's decision.</summary>
    public Task<CaseDetailResponse> OutsideDeclinedAsync(long id, ReasonRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.OutsideDeclined, (c, now) =>
        {
            var party = ActiveThirdParty(c)!;
            party.Status = OutsideWorkStatus.Declined;
            party.FinishedAt = now;
            c.HoldReason = HoldReason.WaitingStaffDecision;
            c.HoldUntil = null;
            return Task.FromResult<string?>($"{party.CompanyName} refused: {request.Reason.Trim()}");
        }, cancellationToken);

    /// <summary>The technician's check failed: the same party does it again (another appointment).</summary>
    public Task<CaseDetailResponse> RedoOutsideWorkAsync(long id, RedoOutsideWorkRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.RedoOutsideWork, (c, _) =>
        {
            var party = ActiveThirdParty(c)!;
            party.Status = OutsideWorkStatus.Working;
            party.RedoCount++;
            party.AppointmentAt = request.AppointmentAt?.ToUniversalTime() ?? party.AppointmentAt;
            notifications.Send(c.AssignedTechnicianId is { } t ? [t] : [],
                $"{Case.CodeOf(c.Id)} \"{c.Title}\": {party.CompanyName} comes back to redo it{When(party.AppointmentAt)}.");
            return Task.FromResult<string?>($"Asked {party.CompanyName} to redo it{When(party.AppointmentAt)}: {request.Reason.Trim()}");
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
            CostBearer = CostBearer.Building,
            StaffId = currentUser.Id,
            IsSurvey = true,
            SubmittedAt = now,
        };
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
