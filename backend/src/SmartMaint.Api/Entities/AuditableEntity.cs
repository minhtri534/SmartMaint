namespace SmartMaint.Api.Entities;

/// <summary>
/// Entity with created_at / updated_at (timestamptz, UTC). Values are set automatically by
/// TimestampInterceptor in Infrastructure — never assign them by hand in services.
/// </summary>
public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
