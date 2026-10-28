using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.Catalog.Dtos;

/// <summary>One item of the service catalogue (group + type) with its defaults.</summary>
public sealed record ServiceTypeResponse(
    long Id,
    string GroupName,
    string Name,
    CasePriority DefaultUrgency,
    bool IsEmergency,
    CostBearer DefaultCostBearer,
    bool PhotoRequired);

public sealed record RoomResponse(long Id, string Name);

public sealed record FloorResponse(long Id, string Name, IReadOnlyList<RoomResponse> Rooms);

public sealed record BuildingResponse(long Id, string Name, IReadOnlyList<FloorResponse> Floors);

public sealed record AssetOptionResponse(long Id, string Name, long RoomId);

/// <summary>A technician the staff member can assign, with how many cases they hold now.</summary>
public sealed record TechnicianOptionResponse(long Id, string FullName, int OpenCases);
