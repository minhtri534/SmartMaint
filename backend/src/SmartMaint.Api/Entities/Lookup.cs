namespace SmartMaint.Api.Entities;

/// <summary>
/// One row of an ERD lookup table (Roles, CaseStatuses...): ID + Name. The ID is the value of a C# enum, so code reads
/// <c>c.Status == CaseStatus.Pending</c> while the database keeps a real FK (cases.status_id -> case_statuses.id).
/// The rows are written by the migration from the enum (LookupExtensions.HasLookupTable) and never change at runtime:
/// a new status = a new enum member + a new migration.
/// </summary>
public sealed class Lookup<TEnum>
    where TEnum : struct, Enum
{
    public TEnum Id { get; set; }

    /// <summary>The same snake_case text the API sends ("in_progress").</summary>
    public string Name { get; set; } = string.Empty;
}
