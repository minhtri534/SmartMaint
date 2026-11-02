using SmartMaint.Api.Entities.Accounts;

namespace SmartMaint.Api.Entities.Core;

/// <summary>
/// ERD: AuditLog(ID, AdminID, TargetType, Operation, OldValue, NewValue, Time). Append-only: never updated or deleted.
/// As drawn in the ERD only Administrator actions are recorded (FK to Admins) and there is no TargetID column, so the
/// changed record is identified inside OldValue / NewValue (JSON with its id). Known gap, see ERD_v5_Thay_Doi_Khi_Code.md.
/// </summary>
public class AuditLog : Entity
{
    public long AdminId { get; set; }
    public Admin Admin { get; set; } = null!;

    /// <summary>ERD: TargetType -> AuditLogTargetType.ID (column target_type_id).</summary>
    public AuditTarget TargetType { get; set; }

    /// <summary>ERD: Operation -> AuditLogTargetOperation.ID (column operation_id).</summary>
    public AuditOperation Operation { get; set; }

    /// <summary>JSON of the record before the change (null on create / sign-in).</summary>
    public string? OldValue { get; set; }

    /// <summary>JSON of the record after the change.</summary>
    public string? NewValue { get; set; }

    /// <summary>ERD: Time.</summary>
    public DateTimeOffset OccurredAt { get; set; }
}

/// <summary>ERD: AuditLogTargetType(ID, Name) — what kind of record was changed.</summary>
public enum AuditTarget
{
    Account = 1,
    PriorityLevel = 2,
}

/// <summary>ERD: AuditLogTargetOperation(ID, Name) — what was done.</summary>
public enum AuditOperation
{
    SignIn = 1,
    Create = 2,
    Update = 3,
    ChangeRole = 4,
    Deactivate = 5,
    Reactivate = 6,
    ResetPassword = 7,
    Unlock = 8,
}
