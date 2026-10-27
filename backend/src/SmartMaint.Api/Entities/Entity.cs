namespace SmartMaint.Api.Entities;

/// <summary>
/// Base class for every table row. PK is a database-generated bigint identity (D-plan convention).
/// Human-readable codes (e.g. CS-000123) live in a separate column on the entities that need one.
/// </summary>
public abstract class Entity
{
    public long Id { get; protected set; }
}
