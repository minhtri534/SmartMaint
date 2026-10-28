using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Users;

namespace SmartMaint.Api.Entities.Cases;

/// <summary>
/// A maintenance case (ERD: Cases). Column names follow the D-plan (*_at, *_id); the ERD name is in the comment.
/// Columns marked NOT IN ERD v5 are what F3 needs and the team still has to add or cut at the ERD meeting
/// (list: docs/database/F3_Cot_Bo_Sung_Cho_ERD.md). The display code CS-000123 is built from the id, not stored.
/// </summary>
public class Case : AuditableEntity
{
    public long RequestingCustomerId { get; set; }
    public User Customer { get; set; } = null!;

    /// <summary>NOT IN ERD v5: where the problem is. The ERD only links Assets, but a customer may not know the asset.</summary>
    public long RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public long? AssetId { get; set; }
    public Asset? Asset { get; set; }

    public long CategoryId { get; set; }
    public CaseCategory Category { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CaseStatus Status { get; set; }

    public long PriorityLevelId { get; set; }
    public PriorityLevel Priority { get; set; } = null!;

    /// <summary>NOT IN ERD v5.</summary>
    public CostBearer CostBearer { get; set; }

    /// <summary>The staff member who approved it (ERD: Staff). Any staff of the building may act; the first to approve is kept.</summary>
    public long? StaffId { get; set; }
    public User? Staff { get; set; }

    /// <summary>The technician holding it now (ERD: AssignedTechnician). The full history is in CaseAssignment.</summary>
    public long? AssignedTechnicianId { get; set; }
    public User? Technician { get; set; }

    /// <summary>ERD: TimeSubmitted.</summary>
    public DateTimeOffset SubmittedAt { get; set; }

    /// <summary>
    /// The SLA clock starts when the technician starts work (doc "Luồng Chính", 28/10). ERD: StartTime.
    /// DueAt (ERD: DueTime) = StartedAt + the priority's SlaHours. Both are cleared on Reopen and set again on the next start.
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? DueAt { get; set; }

    /// <summary>ERD: TimeCompleted.</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>NOT IN ERD v5: needed for the 7-day "not fixed" window.</summary>
    public DateTimeOffset? ClosedAt { get; set; }

    /// <summary>ERD: ActualCompletionHours. Typed by the technician, or measured from StartedAt to CompletedAt.</summary>
    public decimal? ActualHours { get; set; }

    /// <summary>ERD: TimeOutsourced. This case has an outside party (F5); the moment of the hand-over.</summary>
    public DateTimeOffset? OutsourcedAt { get; set; }

    /// <summary>NOT IN ERD v5 (repair record). Parts are written by hand: there is no stock (N2).</summary>
    public string? RootCause { get; set; }
    public string? PartsUsed { get; set; }

    /// <summary>NOT IN ERD v5: filled while Status = OnHold.</summary>
    public HoldReason? HoldReason { get; set; }
    public DateOnly? HoldUntil { get; set; }

    /// <summary>NOT IN ERD v5: the customer's rating, once per case.</summary>
    public int? RatingStars { get; set; }
    public string? RatingComment { get; set; }
    public DateTimeOffset? RatedAt { get; set; }

    public List<CaseImage> Images { get; set; } = [];
    public List<CaseStatusChange> History { get; set; } = [];
    public List<CaseAssignment> Assignments { get; set; } = [];

    public static string CodeOf(long id) => $"CS-{id:D6}";
}

/// <summary>ERD: CaseImages(Image, Notes, Case). Kind (report / before / after) is NOT IN ERD v5.</summary>
public class CaseImage : AuditableEntity
{
    public long CaseId { get; set; }
    public Case Case { get; set; } = null!;
    public CaseImageKind Kind { get; set; }

    /// <summary>A data URL for now. TODO(storage): upload to Supabase Storage (Attachment module) and keep the path.</summary>
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// One status change. ERD: CaseStatusHistory(NewStatus, Case, TimeChanged). FromStatus, ChangedBy and Reason are
/// NOT IN ERD v5; without them nobody can tell who approved, closed or paused a case, or why.
/// </summary>
public class CaseStatusChange : Entity
{
    public long CaseId { get; set; }
    public Case Case { get; set; } = null!;
    public CaseStatus? FromStatus { get; set; }
    public CaseStatus ToStatus { get; set; }
    public long ChangedById { get; set; }
    public User ChangedBy { get; set; } = null!;
    public string? Reason { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
}

/// <summary>
/// Every assignment of a technician, kept after a decline or a reassign (N10).
/// ERD has only Cases.AssignedTechnician + TechnicianRejections; this full history (CaseAssignments) is proposed.
/// </summary>
public class CaseAssignment : Entity
{
    public long CaseId { get; set; }
    public Case Case { get; set; } = null!;
    public long TechnicianId { get; set; }
    public User Technician { get; set; } = null!;
    public long AssignedById { get; set; }
    public User AssignedBy { get; set; } = null!;
    public DateTimeOffset AssignedAt { get; set; }
    public AssignmentAnswer Response { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }
    public string? DeclineReason { get; set; }
}
