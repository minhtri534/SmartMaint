using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.PriorityLevels.Dtos;

public sealed record PriorityLevelResponse(long Id, CasePriority Code, string Name, string? Description, int SlaHours);

/// <summary>ADM-07: hours from the start of work to the due time.</summary>
public sealed record UpdateSlaRequest(int SlaHours);
