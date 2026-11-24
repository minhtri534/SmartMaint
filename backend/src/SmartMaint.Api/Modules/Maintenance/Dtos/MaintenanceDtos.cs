using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Maintenance;

namespace SmartMaint.Api.Modules.Maintenance.Dtos;

// F2. Names match frontend/src/features/maintenance/types.ts.

// ----- checklist templates (ERD: CaseChecklists, ChecklistTasks) --------------------------------------------------

public sealed record ChecklistItemRequest(string Name, string? Description);

public sealed record ChecklistRequest(string Name, string? Description, IReadOnlyList<ChecklistItemRequest> Tasks);

public sealed record ChecklistItemResponse(long Id, string Name, string? Description);

public sealed record ChecklistResponse(
    long Id,
    string Name,
    string? Description,
    IReadOnlyList<ChecklistItemResponse> Tasks,
    int UsedByPlans);

// ----- plans (ERD: MaintenanceTriggers + MaintenanceWorkOrderDetails + MaintenanceWorkOrderAssets) ----------------
// Meeting note 18/11, like UpKeep: a plan = one work-order template + one or more triggers (schedules) with their assets.

/// <summary>One asset of a trigger and its default technician (empty = the staff assigns each work order).</summary>
public sealed record TriggerAssetRequest(long AssetId, long? TechnicianId);

/// <summary>
/// One trigger: every <see cref="Frequency"/> <see cref="FrequencyType"/>s from <see cref="StartDate"/> until
/// <see cref="EndDate"/> (empty = no end). <see cref="Id"/> = an existing trigger of the plan being edited, null = new.
/// </summary>
public sealed record TriggerRequest(
    long? Id,
    int Frequency,
    FrequencyUnit FrequencyType,
    int LeadDays,
    DateOnly StartDate,
    DateOnly? EndDate,
    IReadOnlyList<TriggerAssetRequest> Assets);

/// <summary>PM-02. The template every work order of the plan gets, and its triggers (assets of one building).</summary>
public sealed record PlanRequest(
    string Name,
    string? Description,
    long ChecklistId,
    decimal? EstimatedHours,
    CasePriority Priority,
    IReadOnlyList<TriggerRequest> Triggers);

public sealed record RejectPlanRequest(string Reason);

public enum PlanAction
{
    Edit,
    Approve,
    Reject,
    Pause,
    Resume,
    Archive,
}

public sealed record PersonRef(long Id, string FullName);

/// <summary>"Every 3 months".</summary>
public sealed record PlanEvery(int Frequency, FrequencyUnit FrequencyType);

public sealed record PlanAssetResponse(
    long AssetId,
    string AssetName,
    string RoomName,
    PersonRef? Technician,
    DateOnly StartDate,
    DateOnly? LastDueDate,
    DateOnly? NextDueDate,
    bool IsRetired);

/// <summary>One trigger of a plan. Expired = it has an end day and no due day is left before it.</summary>
public sealed record PlanTriggerResponse(
    long Id,
    int Frequency,
    FrequencyUnit FrequencyType,
    int LeadDays,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateOnly? NextDueDate,
    bool Expired,
    IReadOnlyList<PlanAssetResponse> Assets);

/// <summary>
/// One row of PM-01. Expired = running or paused, but every trigger is past its end day: no work order is made until the
/// staff moves an end day later.
/// </summary>
public record PlanSummaryResponse(
    long Id,
    string Name,
    TriggerStatus Status,
    bool Expired,
    IReadOnlyList<PlanEvery> Every,
    long BuildingId,
    string BuildingName,
    int AssetCount,
    DateOnly? NextDueDate,
    PersonRef Staff,
    PersonRef? ApprovingManager);

/// <summary>PM-03. The work orders are on their own tab (<see cref="PlanWorkOrderResponse"/>).</summary>
public sealed record PlanDetailResponse(
    long Id,
    string Name,
    TriggerStatus Status,
    bool Expired,
    IReadOnlyList<PlanEvery> Every,
    long BuildingId,
    string BuildingName,
    int AssetCount,
    DateOnly? NextDueDate,
    PersonRef Staff,
    PersonRef? ApprovingManager,
    string? Description,
    long ChecklistId,
    string ChecklistName,
    int ChecklistItems,
    decimal? EstimatedHours,
    CasePriority Priority,
    string? RejectReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastPaused,
    DateTimeOffset? LastTriggered,
    DateTimeOffset? ArchivedAt,
    IReadOnlyList<PlanTriggerResponse> Triggers,
    int WorkOrderCount,
    IReadOnlyList<PlanAction> AllowedActions)
    : PlanSummaryResponse(
        Id, Name, Status, Expired, Every, BuildingId, BuildingName, AssetCount, NextDueDate, Staff, ApprovingManager);

/// <summary>A work order the plan made (a case): the "Work orders" tab, newest due day first.</summary>
public sealed record PlanWorkOrderResponse(
    long Id,
    string Code,
    long TriggerId,
    string AssetName,
    string? TechnicianName,
    CaseStatus Status,
    DateTimeOffset? DueAt,
    DateTimeOffset? ClosedAt);
