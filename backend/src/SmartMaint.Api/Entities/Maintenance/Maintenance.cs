using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Entities.Maintenance;

// F2 (Trí) — preventive maintenance. Tables as in the ERD (Drive, logical page); NOT IN ERD v5 columns are listed in
// docs/database/ERD_v5_Thay_Doi_Khi_Code.md.
//
// How the tables fit: a plan (MaintenanceTrigger) is approved by a manager; it has work-order details (which checklist,
// how long, how urgent), each applied to some assets (MaintenanceWorkOrderAssets, with the default technician and the
// first due day). Every period the job makes one Case per asset (Cases.TriggerWorkOrder) with one CaseTask per checklist
// item; the technician marks each task.

/// <summary>ERD: FrequencyType(ID, Name) — the unit of MaintenanceTriggers.Frequency.</summary>
public enum FrequencyUnit
{
    Day = 1,
    Week = 2,
    Month = 3,
    Year = 4,
}

/// <summary>ERD: TriggerStatuses(ID, Name). E12: "pending / approved / rejected..." are rows, not columns.</summary>
public enum TriggerStatus
{
    PendingApproval = 1,
    Active = 2,
    Rejected = 3,
    Paused = 4,
    Ended = 5,
}

/// <summary>ERD: TaskStatuses(ID, Name) — the result of one checklist item (ERD review #13: a result, not progress).</summary>
public enum CaseTaskStatus
{
    Pending = 1,
    Pass = 2,
    Fail = 3,
    NotApplicable = 4,
}

/// <summary>ERD: CaseChecklists(ID, Name, Description, Category) — a checklist template for a service type.</summary>
public class CaseChecklist : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>ERD: Category -> CaseCategories. The generated cases get this service type.</summary>
    public long CategoryId { get; set; }
    public CaseCategory Category { get; set; } = null!;

    public List<ChecklistTask> Tasks { get; set; } = [];
}

/// <summary>ERD: ChecklistTasks(ID, Name, Description, Checklist) — one item of a template.</summary>
public class ChecklistTask : AuditableEntity
{
    public long ChecklistId { get; set; }
    public CaseChecklist Checklist { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// ERD: MaintenanceTriggers(ID, Frequency, FrequencyType, Staff, ApprovingManager, Status, LastPaused, LastTriggered) —
/// a preventive maintenance plan. E10: a manager approves it before it runs.
/// </summary>
public class MaintenanceTrigger : AuditableEntity
{
    /// <summary>Every <see cref="Frequency"/> <see cref="FrequencyType"/>s, e.g. every 3 months.</summary>
    public int Frequency { get; set; }

    /// <summary>ERD: FrequencyType -> FrequencyType.ID (column frequency_type_id).</summary>
    public FrequencyUnit FrequencyType { get; set; }

    /// <summary>ERD: Staff — who made the plan.</summary>
    public long StaffId { get; set; }
    public Staff Staff { get; set; } = null!;

    /// <summary>ERD: ApprovingManager — set when a manager approves or rejects.</summary>
    public long? ApprovingManagerId { get; set; }
    public Manager? ApprovingManager { get; set; }

    /// <summary>ERD: Status -> TriggerStatuses.ID (column status_id).</summary>
    public TriggerStatus Status { get; set; }

    public DateTimeOffset? LastPaused { get; set; }

    /// <summary>When the job last made work orders for this plan.</summary>
    public DateTimeOffset? LastTriggered { get; set; }

    /// <summary>NOT IN ERD v5: work orders are made this many days before the due day (Leader 02/11: 3).</summary>
    public int LeadDays { get; set; } = 3;

    /// <summary>NOT IN ERD v5 (ERD review #10): why the manager rejected it, shown to the staff member.</summary>
    public string? RejectReason { get; set; }

    public List<MaintenanceWorkOrderDetail> Details { get; set; } = [];
}

/// <summary>ERD: MaintenanceWorkOrderDetails(ID, Trigger, Checklist, EstimatedHours, PriorityLevel).</summary>
public class MaintenanceWorkOrderDetail : AuditableEntity
{
    public long TriggerId { get; set; }
    public MaintenanceTrigger Trigger { get; set; } = null!;

    public long ChecklistId { get; set; }
    public CaseChecklist Checklist { get; set; } = null!;

    public decimal? EstimatedHours { get; set; }

    public long PriorityLevelId { get; set; }
    public PriorityLevel PriorityLevel { get; set; } = null!;

    public List<MaintenanceWorkOrderAsset> Assets { get; set; } = [];
}

/// <summary>
/// ERD: MaintenanceWorkOrderAssets(WorkOrderDetail, Asset, Technician, StartTime, EndTime) — one asset of a plan.
/// StartTime = the first due day of this asset, EndTime = the last day the plan applies to it (ERD review #3).
/// The schedule is fixed (Leader 02/11): due days are StartTime + n × frequency, late work does not move them.
/// </summary>
public class MaintenanceWorkOrderAsset
{
    public long WorkOrderDetailId { get; set; }
    public MaintenanceWorkOrderDetail WorkOrderDetail { get; set; } = null!;

    public long AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    /// <summary>The default technician; empty = the staff member assigns each work order.</summary>
    public long? TechnicianId { get; set; }
    public Technician? Technician { get; set; }

    public DateOnly StartTime { get; set; }
    public DateOnly? EndTime { get; set; }

    /// <summary>NOT IN ERD v5 (ERD review #3): the due day of the last work order made for this asset.</summary>
    public DateOnly? LastDueDate { get; set; }
}

/// <summary>ERD: CaseTasks(ID, Name, Description, Status, Case) — one checklist item of a generated work order.</summary>
public class CaseTask : AuditableEntity
{
    public long CaseId { get; set; }
    public Case Case { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>ERD: Status -> TaskStatuses.ID (column status_id).</summary>
    public CaseTaskStatus Status { get; set; }

    /// <summary>
    /// NOT IN ERD v5 (ERD review #6): the repair case made from this failed item (BR-07). Empty for a failed item the
    /// technician fixed on the spot.
    /// </summary>
    public long? RepairCaseId { get; set; }
    public Case? RepairCase { get; set; }

    public List<TaskNote> Notes { get; set; } = [];
    public List<TaskImage> Images { get; set; } = [];
}

/// <summary>ERD: TaskNotes(ID, Note, Task).</summary>
public class TaskNote : AuditableEntity
{
    public long TaskId { get; set; }
    public CaseTask Task { get; set; } = null!;
    public string Note { get; set; } = string.Empty;
}

/// <summary>ERD: TaskImages(ID, Image, Notes, Task). Image = storage path, like CaseImages.</summary>
public class TaskImage : AuditableEntity
{
    public long TaskId { get; set; }
    public CaseTask Task { get; set; } = null!;
    public string Image { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
