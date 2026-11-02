namespace SmartMaint.Api.Modules.Facilities.Dtos;

// AST-01 Facility tree. Names match frontend/src/features/facilities/types.ts.

public sealed record RoomNodeResponse(
    long Id,
    string RoomNumber,
    bool IsCommonArea,
    int AssetCount,
    IReadOnlyList<string> Tenants);

/// <summary>Name = "Floor 5" (display), FloorNumber = "5" (ERD: Floors.FloorNumber).</summary>
public sealed record FloorNodeResponse(long Id, string FloorNumber, string Name, IReadOnlyList<RoomNodeResponse> Rooms);

public sealed record BuildingNodeResponse(long Id, string Name, string? Address, IReadOnlyList<FloorNodeResponse> Floors);

/// <summary>ERD: Buildings(Name, Address).</summary>
public sealed record BuildingRequest(string Name, string? Address);

/// <summary>ERD: Floors(FloorNumber, Building). The building of a floor never changes.</summary>
public sealed record FloorRequest(long BuildingId, string FloorNumber);

/// <summary>ERD: Rooms(RoomNumber, Floor) + NOT IN ERD v5 IsCommonArea. The floor of a room never changes.</summary>
public sealed record RoomRequest(long FloorId, string RoomNumber, bool IsCommonArea);
