using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>Entity -> DTO, by hand (B6). The case must be loaded with the includes of CaseService.WithRefs.</summary>
internal static class CaseMapper
{
    public static CaseSummaryResponse ToSummary(Case c) => new(
        c.Id,
        Case.CodeOf(c.Id),
        c.Title,
        c.Status,
        c.Priority.Code,
        ServiceTypeName(c.Category),
        ToLocation(c),
        ToPersonOrNull(c.Customer),
        ToPersonOrNull(c.Staff),
        ToPersonOrNull(c.Technician),
        c.SubmittedAt,
        c.DueAt,
        c.StartedAt,
        c.CompletedAt,
        IsEmergencyFlow(c));

    /// <summary>
    /// Needs Images, History(ChangedBy), Rejections(Technician) and Asset loaded too.
    /// <paramref name="photoUrls"/> maps CaseImages.Image (storage path) to a URL the browser can open.
    /// </summary>
    public static CaseDetailResponse ToDetail(
        Case c, IReadOnlyList<CaseAction> allowedActions, IReadOnlyDictionary<string, string> photoUrls, bool showCosts)
    {
        var history = c.History.OrderBy(h => h.ChangedAt).ThenBy(h => h.Id).ToList();
        return new CaseDetailResponse(
            c.Id,
            Case.CodeOf(c.Id),
            c.Title,
            c.Status,
            c.Priority.Code,
            ServiceTypeName(c.Category),
            ToLocation(c),
            ToPersonOrNull(c.Customer),
            ToPersonOrNull(c.Staff),
            ToPersonOrNull(c.Technician),
            c.SubmittedAt,
            c.DueAt,
            c.StartedAt,
            c.CompletedAt,
            IsEmergencyFlow(c),
            c.Description,
            c.CategoryId,
            c.Asset is null ? null : new AssetResponse(c.Asset.Id, c.Asset.Name, c.Asset.RoomId),
            c.CostBearer,
            c.Images
                .OrderBy(i => i.Id)
                .Where(i => photoUrls.ContainsKey(i.Image))
                .Select(i => new PhotoResponse(i.Id, photoUrls[i.Image], i.Kind))
                .ToList(),
            // The ERD keeps only the new status; the previous one is the row before.
            history
                .Select((h, i) => new StatusChangeResponse(
                    h.ChangedAt, i == 0 ? null : history[i - 1].NewStatus, h.NewStatus, ToPerson(h.ChangedBy), h.Reason))
                .ToList(),
            c.Rejections
                .OrderBy(r => r.RejectedAt)
                .Select(r => new RejectionResponse(ToPerson(r.Technician), r.RejectedAt, r.Reason))
                .ToList(),
            c.RootCause is not null && c.ActualCompletionHours is { } hours
                ? new RepairReportResponse(c.RootCause, c.PartsUsed, hours)
                : null,
            c.RatingStars is { } stars && c.RatedAt is { } ratedAt ? new RatingResponse(stars, c.RatingComment, ratedAt) : null,
            c.Status == CaseStatus.OnHold && c.HoldReason is { } reason && c.HoldUntil is { } until
                ? new HoldResponse(reason, until)
                : null,
            showCosts ? c.Costs.OrderBy(x => x.Id).Select(x => new CostResponse(x.Id, x.Name, x.Description, x.Amount)).ToList() : null,
            showCosts && c.Costs.Count > 0 ? c.Costs.Sum(x => x.Amount) : null,
            c.NotFixedReason is { } notFixed && c.NotFixedAt is { } reportedAt ? new NotFixedResponse(notFixed, reportedAt) : null,
            allowedActions);
    }

    public static bool IsEmergencyFlow(Case c) => c.Priority.Code == CasePriority.Emergency;

    public static string ServiceTypeName(CaseCategory category) => $"{category.GroupName} · {category.Name}";

    private static PersonResponse ToPerson(Account account) => new(account.Id, account.FullName);

    private static PersonResponse? ToPersonOrNull(Account? account) => account is null ? null : ToPerson(account);

    private static LocationResponse ToLocation(Case c) => new(
        c.Room.Floor.BuildingId, c.Room.Floor.Building.Name, c.Room.Floor.DisplayName, c.RoomId, c.Room.RoomNumber);
}
