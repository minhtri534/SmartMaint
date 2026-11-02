using SmartMaint.Api.Common.Errors;

namespace SmartMaint.Api.Modules.Facilities;

/// <summary>Every error the Facilities module can return. Codes are part of the API contract.</summary>
internal static class FacilityErrors
{
    public static AppException NotFound() =>
        new(ErrorKind.NotFound, "facility.not_found", "This building, floor or room does not exist.");

    public static AppException BuildingNameTaken() =>
        new(ErrorKind.Conflict, "facility.building_name_taken", "Another building already has this name.");

    public static AppException FloorTaken() =>
        new(ErrorKind.Conflict, "facility.floor_taken", "This building already has a floor with this number.");

    public static AppException RoomTaken() =>
        new(ErrorKind.Conflict, "facility.room_taken", "This floor already has a room with this number or name.");

    public static AppException NotEmpty(string what) =>
        new(ErrorKind.Conflict, "facility.not_empty", $"This {what} still has {(what == "room" ? "assets, tenants or cases" : "content")}: it cannot be deleted.");

    public static AppException RoomRented() =>
        new(ErrorKind.Conflict, "facility.room_rented", "A customer rents this room, so it cannot become a common area.");
}
