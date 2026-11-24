using SmartMaint.Api.Entities.Buildings;

namespace SmartMaint.Api.Entities.Cases;

/// <summary>
/// One row per urgency level with its SLA length. ERD: PriorityLevels(ID, Name, Description); Code and SlaHours are
/// NOT IN ERD v5. SLA hours are wall-clock for now; the real clock should count office hours (BR-08, O04).
/// </summary>
public class PriorityLevel : AuditableEntity
{
    /// <summary>Fixed key the code works with (emergency / high / normal / low); Name is the label.</summary>
    public CasePriority Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SlaHours { get; set; }
}

/// <summary>
/// The fault categories the customer picks from. ERD: CaseCategories(Name, Description). NOT IN ERD v5: AssetTypeId,
/// ArchivedAt, GroupName. Meeting note 18/11: IsEmergency, DefaultPriority and PhotoRequired no longer do anything; the
/// columns stay until the old API is gone.
/// </summary>
public class CaseCategory : AuditableEntity, IArchivable
{
    public string GroupName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>The asset type this fault belongs to; empty = every asset type.</summary>
    public long? AssetTypeId { get; set; }
    public AssetType? AssetType { get; set; }

    /// <summary>Archived: not offered for new requests, kept on the cases that used it.</summary>
    public DateTimeOffset? ArchivedAt { get; set; }

    public bool IsEmergency { get; set; }

    public long DefaultPriorityId { get; set; }
    public PriorityLevel DefaultPriority { get; set; } = null!;

    public bool PhotoRequired { get; set; }
}
