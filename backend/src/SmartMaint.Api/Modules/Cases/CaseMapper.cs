using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Users;
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
        ToPerson(c.Customer),
        c.Staff is null ? null : ToPerson(c.Staff),
        c.Technician is null ? null : ToPerson(c.Technician),
        c.SubmittedAt,
        c.DueAt,
        c.StartedAt,
        c.CompletedAt,
        IsEmergencyFlow(c));

    /// <summary>Needs Images, History(ChangedBy), Assignments(Technician, AssignedBy) and Asset loaded too.</summary>
    public static CaseDetailResponse ToDetail(Case c, IReadOnlyList<CaseAction> allowedActions) => new(
        c.Id,
        Case.CodeOf(c.Id),
        c.Title,
        c.Status,
        c.Priority.Code,
        ServiceTypeName(c.Category),
        ToLocation(c),
        ToPerson(c.Customer),
        c.Staff is null ? null : ToPerson(c.Staff),
        c.Technician is null ? null : ToPerson(c.Technician),
        c.SubmittedAt,
        c.DueAt,
        c.StartedAt,
        c.CompletedAt,
        IsEmergencyFlow(c),
        c.Description,
        c.CategoryId,
        c.Asset is null ? null : new AssetResponse(c.Asset.Id, c.Asset.Name, c.Asset.RoomId),
        c.CostBearer,
        c.Images.OrderBy(i => i.Id).Select(i => new PhotoResponse(i.Id, i.Content, i.Kind)).ToList(),
        c.History
            .OrderBy(h => h.ChangedAt).ThenBy(h => h.Id)
            .Select(h => new StatusChangeResponse(h.ChangedAt, h.FromStatus, h.ToStatus, ToPerson(h.ChangedBy), h.Reason))
            .ToList(),
        c.Assignments
            .OrderBy(a => a.Id)
            .Select(a => new AssignmentResponse(
                ToPerson(a.Technician), ToPerson(a.AssignedBy), a.AssignedAt, a.Response, a.RespondedAt, a.DeclineReason))
            .ToList(),
        c.RootCause is not null && c.ActualHours is { } hours
            ? new RepairReportResponse(c.RootCause, c.PartsUsed, hours)
            : null,
        c.RatingStars is { } stars && c.RatedAt is { } ratedAt ? new RatingResponse(stars, c.RatingComment, ratedAt) : null,
        c.Status == CaseStatus.OnHold && c.HoldReason is { } reason && c.HoldUntil is { } until
            ? new HoldResponse(reason, until)
            : null,
        allowedActions);

    public static bool IsEmergencyFlow(Case c) => c.Priority.Code == CasePriority.Emergency;

    public static string ServiceTypeName(CaseCategory category) => $"{category.GroupName} · {category.Name}";

    private static PersonResponse ToPerson(User user) => new(user.Id, user.FullName);

    private static LocationResponse ToLocation(Case c) => new(
        c.Room.Floor.BuildingId, c.Room.Floor.Building.Name, c.Room.Floor.Name, c.RoomId, c.Room.Name);
}
