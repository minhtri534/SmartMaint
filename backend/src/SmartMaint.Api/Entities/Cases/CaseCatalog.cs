namespace SmartMaint.Api.Entities.Cases;

/// <summary>
/// One row per urgency level with its SLA length. ERD: PriorityLevels(Name, Description); SlaHours is NOT IN ERD v5.
/// SLA hours are wall-clock for now; the real clock should count office hours (BR-08, O04).
/// </summary>
public class PriorityLevel : AuditableEntity
{
    public CasePriority Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SlaHours { get; set; }
}

/// <summary>
/// The service catalogue the customer picks from (N24): group "Water", type "Leaking pipe or tap".
/// ERD: CaseCategories(Name, Description). GroupName, IsEmergency, DefaultPriority, PhotoRequired and
/// DefaultCostBearer are NOT IN ERD v5.
/// </summary>
public class CaseCategory : AuditableEntity
{
    public string GroupName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>In the emergency list: the case goes to F4 as soon as it is created (N26).</summary>
    public bool IsEmergency { get; set; }

    public long DefaultPriorityId { get; set; }
    public PriorityLevel DefaultPriority { get; set; } = null!;
    public CostBearer DefaultCostBearer { get; set; }

    /// <summary>E.g. a leak needs a photo, so fewer requests are rejected for missing information (O05).</summary>
    public bool PhotoRequired { get; set; }
}
