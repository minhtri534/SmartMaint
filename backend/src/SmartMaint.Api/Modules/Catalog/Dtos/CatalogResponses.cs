using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.Catalog.Dtos;

/// <summary>One item of the service catalogue (group + type) with its defaults.</summary>
public sealed record ServiceTypeResponse(
    long Id,
    string GroupName,
    string Name,
    CasePriority DefaultUrgency,
    bool IsEmergency,
    bool PhotoRequired);

public sealed record RoomResponse(long Id, string Name, bool IsCommonArea);

public sealed record FloorResponse(long Id, string Name, IReadOnlyList<RoomResponse> Rooms);

public sealed record BuildingResponse(long Id, string Name, IReadOnlyList<FloorResponse> Floors);

public sealed record AssetOptionResponse(long Id, string Name, long RoomId);

/// <summary>
/// A technician the staff member can choose (meeting 07/11): their specialties and their schedule, i.e. the open jobs
/// they hold with the planned window / deadline, so the staff can see who is free and who fits.
/// </summary>
public sealed record TechnicianOptionResponse(
    long Id, string FullName, int OpenCases, IReadOnlyList<string> Specialties, IReadOnlyList<TechnicianJobResponse> Jobs);

/// <summary>
/// One open job of a technician. Title is empty when the case is in a building the caller does not manage (B3: the
/// staff only sees that the technician is busy then).
/// </summary>
public sealed record TechnicianJobResponse(
    long Id,
    string Code,
    string? Title,
    CaseStatus Status,
    CasePriority Urgency,
    DateTimeOffset? PlannedStartAt,
    DateTimeOffset? PlannedEndAt,
    DateTimeOffset? DueAt);

public sealed record SpecialtyOptionResponse(long Id, string Name);

public sealed record AssetTypeOptionResponse(long Id, string Name, long CategoryId, string CategoryName);
