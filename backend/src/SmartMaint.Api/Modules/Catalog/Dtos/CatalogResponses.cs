using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.Catalog.Dtos;

/// <summary>A fault category people can choose. AssetTypeId empty = it applies to every asset type.</summary>
public sealed record ServiceTypeResponse(long Id, string GroupName, string Name, long? AssetTypeId);

public sealed record RoomResponse(long Id, string Name);

public sealed record FloorResponse(long Id, string Name, IReadOnlyList<RoomResponse> Rooms);

public sealed record BuildingResponse(long Id, string Name, IReadOnlyList<FloorResponse> Floors);

public sealed record AssetOptionResponse(long Id, string Name, long RoomId, long TypeId);

/// <summary>
/// A technician the staff member can choose (meeting 07/11): their specialties and their schedule, i.e. the open jobs
/// they hold with the planned window / deadline, so the staff can see who is free and who fits.
/// </summary>
public sealed record TechnicianOptionResponse(
    long Id, string FullName, int OpenCases, IReadOnlyList<string> Specialties, IReadOnlyList<TechnicianJobResponse> Jobs, bool Fits);

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

/// <summary>A manufacturer to choose in a change list (meeting note 18/11: the staff now fill in assets).</summary>
public sealed record ManufacturerOptionResponse(long Id, string Name);
