using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Maintenance;

namespace SmartMaint.Api.Modules.Maintenance.Dtos;

// F2. Names match frontend/src/features/maintenance/types.ts.

// ----- checklist templates (ERD: CaseChecklists, ChecklistTasks) --------------------------------------------------

public sealed record ChecklistItemRequest(string Name, string? Description);

public sealed record ChecklistRequest(string Name, string? Description, long CategoryId, IReadOnlyList<ChecklistItemRequest> Tasks);

public sealed record ChecklistItemResponse(long Id, string Name, string? Description);

public sealed record ChecklistResponse(
    long Id,
    string Name,
    string? Description,
    long CategoryId,
    string CategoryName,
    IReadOnlyList<ChecklistItemResponse> Tasks,
    int UsedByPlans);

// ----- plans (ERD: MaintenanceTriggers + MaintenanceWorkOrderDetails + MaintenanceWorkOrderAssets) ----------------

/// <summary>One asset of a plan: its default technician (optional) and its first / last due day.</summary>
public sealed record PlanAssetRequest(long AssetId, long? TechnicianId, DateOnly StartDate, DateOnly? EndDate);

/// <summary>PM-02. One plan = one checklist applied to assets of one building (one work-order detail).</summary>
public sealed record PlanRequest(
    int Frequency,
    FrequencyUnit FrequencyType,
    int LeadDays,
    long ChecklistId,
    decimal? EstimatedHours,
    CasePriority Priority,
    IReadOnlyList<PlanAssetRequest> Assets);

public sealed record RejectPlanRequest(string Reason);

public enum PlanAction
{
    Edit,
    Approve,
    Reject,
    Pause,
    Resume,
    End,
}

public sealed record PersonRef(long Id, string FullName);

public sealed record PlanAssetResponse(
    long AssetId,
    string AssetName,
    string RoomName,
    PersonRef? Technician,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateOnly? LastDueDate,
    DateOnly? NextDueDate,
    bool IsRetired);

/// <summary>One row of PM-01.</summary>
public record PlanSummaryResponse(
    long Id,
    string Name,
    TriggerStatus Status,
    int Frequency,
    FrequencyUnit FrequencyType,
    long BuildingId,
    string BuildingName,
    int AssetCount,
    DateOnly? NextDueDate,
    PersonRef Staff,
    PersonRef? ApprovingManager);

/// <summary>A work order made from the plan (a case).</summary>
public sealed record PlanWorkOrderResponse(
    long Id, string Code, string AssetName, CaseStatus Status, DateTimeOffset? DueAt, DateTimeOffset? ClosedAt);

/// <summary>PM-03.</summary>
public sealed record PlanDetailResponse(
    long Id,
    string Name,
    TriggerStatus Status,
    int Frequency,
    FrequencyUnit FrequencyType,
    long BuildingId,
    string BuildingName,
    int AssetCount,
    DateOnly? NextDueDate,
    PersonRef Staff,
    PersonRef? ApprovingManager,
    int LeadDays,
    long ChecklistId,
    string ChecklistName,
    decimal? EstimatedHours,
    CasePriority Priority,
    string? RejectReason,
    DateTimeOffset? LastPaused,
    DateTimeOffset? LastTriggered,
    IReadOnlyList<PlanAssetResponse> Assets,
    IReadOnlyList<PlanWorkOrderResponse> WorkOrders,
    IReadOnlyList<PlanAction> AllowedActions)
    : PlanSummaryResponse(
        Id, Name, Status, Frequency, FrequencyType, BuildingId, BuildingName, AssetCount, NextDueDate, Staff, ApprovingManager);
