using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.Cases.Dtos;

/// <summary>Why an officer rejects a case. "Duplicate" replaces the old merge: the customer is told a case already exists (28/10).</summary>
public enum RejectReason
{
    MissingInfo,
    OutOfScope,
    Duplicate,
}

/// <summary>EMP-02. Photos are image data URLs; the API stores them in Supabase Storage and keeps the path.</summary>
public sealed record CreateCaseRequest(
    string Title,
    string Description,
    long ServiceTypeId,
    long RoomId,
    long? AssetId,
    IReadOnlyList<string>? Photos);

/// <summary>DLG-01: the staff member confirms the urgency and who pays.</summary>
public sealed record ApproveCaseRequest(CasePriority Urgency, CostBearer CostBearer);

/// <summary>DLG-02.</summary>
public sealed record RejectCaseRequest(RejectReason Reason, string Note);

/// <summary>DLG-04.</summary>
public sealed record AssignCaseRequest(long TechnicianId);

/// <summary>Cancel (DLG-12), decline (DLG-09) and reopen (DLG-07) only need a written reason.</summary>
public sealed record ReasonRequest(string Reason);

/// <summary>DLG-10: pause the work with a fixed reason and the day it should continue.</summary>
public sealed record HoldCaseRequest(HoldReason Reason, DateOnly ExpectedResumeOn);

/// <summary>
/// DLG-11: the repair record. LaborHours empty = the system measures start to finish (doc "Luồng Chính").
/// Parts = what the technician bought (cost lines); the labour line is added by the system.
/// </summary>
public sealed record CompleteWorkRequest(
    string RootCause,
    string? PartsUsed,
    decimal? LaborHours,
    IReadOnlyList<string>? BeforePhotos,
    IReadOnlyList<string> AfterPhotos,
    IReadOnlyList<CostLineRequest>? Parts = null);

/// <summary>DLG-14.</summary>
public sealed record RateCaseRequest(int Stars, string? Comment);

/// <summary>One cost line (ERD: Costs). Amount in VND.</summary>
public sealed record CostLineRequest(string Name, string? Description, decimal Amount);

/// <summary>Staff gives the work to another technician (not a decline). The new one must accept again.</summary>
public sealed record ReassignCaseRequest(long TechnicianId, string Reason);

/// <summary>O08: the problem turned out more (or less) urgent. The deadline follows the new level.</summary>
public sealed record ChangeUrgencyRequest(CasePriority Urgency, string Reason);

/// <summary>Why a deadline moves (O09: a fixed list, so the statistics can count them).</summary>
public enum ExtendDueReason
{
    WaitingParts,
    CustomerRescheduled,
    LargerThanExpected,
    Other,
}

/// <summary>O09: only before the deadline passes, and only later.</summary>
public sealed record ExtendDueRequest(DateTimeOffset NewDueAt, ExtendDueReason Reason, string? Note);

/// <summary>Staff corrects the cost lines before closing.</summary>
public sealed record UpdateCostsRequest(IReadOnlyList<CostLineRequest> Lines);
