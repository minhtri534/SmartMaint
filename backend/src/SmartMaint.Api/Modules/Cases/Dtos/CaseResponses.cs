using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.Cases.Dtos;

// Response shapes of the Cases module. Names match frontend/src/features/cases/types.ts (camelCase JSON, snake_case enums).

public sealed record PersonResponse(long Id, string FullName);

public sealed record LocationResponse(long BuildingId, string BuildingName, string FloorName, long RoomId, string RoomName);

public sealed record AssetResponse(long Id, string Name, long RoomId);

public sealed record PhotoResponse(long Id, string Url, CaseImageKind Kind);

public sealed record StatusChangeResponse(
    DateTimeOffset At, CaseStatus? From, CaseStatus To, PersonResponse By, string? Reason);

public sealed record AssignmentResponse(
    PersonResponse Technician,
    PersonResponse AssignedBy,
    DateTimeOffset AssignedAt,
    AssignmentAnswer Response,
    DateTimeOffset? RespondedAt,
    string? DeclineReason);

public sealed record RepairReportResponse(string RootCause, string? PartsUsed, decimal LaborHours);

public sealed record RatingResponse(int Stars, string? Comment, DateTimeOffset RatedAt);

public sealed record HoldResponse(HoldReason Reason, DateOnly ExpectedResumeOn);

/// <summary>One row of a case list.</summary>
public record CaseSummaryResponse(
    long Id,
    string Code,
    string Title,
    CaseStatus Status,
    CasePriority Urgency,
    string ServiceTypeName,
    LocationResponse Location,
    PersonResponse Requester,
    PersonResponse? Staff,
    PersonResponse? Technician,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? DueAt,
    DateTimeOffset? SlaStartedAt,
    DateTimeOffset? CompletedAt,
    bool IsEmergencyFlow);

/// <summary>
/// A whole case. <see cref="AllowedActions"/> is computed by the server for the caller (state machine + their role
/// + their buildings), so the web only draws the buttons: it never re-implements the rules.
/// </summary>
public sealed record CaseDetailResponse(
    long Id,
    string Code,
    string Title,
    CaseStatus Status,
    CasePriority Urgency,
    string ServiceTypeName,
    LocationResponse Location,
    PersonResponse Requester,
    PersonResponse? Staff,
    PersonResponse? Technician,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? DueAt,
    DateTimeOffset? SlaStartedAt,
    DateTimeOffset? CompletedAt,
    bool IsEmergencyFlow,
    string Description,
    long ServiceTypeId,
    AssetResponse? Asset,
    CostBearer CostBearer,
    IReadOnlyList<PhotoResponse> Photos,
    IReadOnlyList<StatusChangeResponse> History,
    IReadOnlyList<AssignmentResponse> Assignments,
    RepairReportResponse? RepairReport,
    RatingResponse? Rating,
    HoldResponse? Hold,
    IReadOnlyList<CaseAction> AllowedActions)
    : CaseSummaryResponse(
        Id, Code, Title, Status, Urgency, ServiceTypeName, Location, Requester, Staff, Technician,
        SubmittedAt, DueAt, SlaStartedAt, CompletedAt, IsEmergencyFlow);
