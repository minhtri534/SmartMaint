using SmartMaint.Api.Common.Errors;

namespace SmartMaint.Api.Modules.Assets;

/// <summary>Every error the Assets module can return. Codes are part of the API contract.</summary>
internal static class AssetErrors
{
    // Also returned for an asset outside the caller's buildings (B3).
    public static AppException NotFound() =>
        new(ErrorKind.NotFound, "asset.not_found", "Asset not found.");

    public static AppException UnknownRoom() =>
        new(ErrorKind.BadRequest, "asset.unknown_room", "Choose the room of the asset.");

    public static AppException UnknownType() =>
        new(ErrorKind.BadRequest, "asset.unknown_type", "Choose the asset type.");

    public static AppException UnknownManufacturer() =>
        new(ErrorKind.BadRequest, "asset.unknown_manufacturer", "This manufacturer does not exist.");

    public static AppException NameTaken() =>
        new(ErrorKind.Conflict, "asset.name_taken", "This room already has an asset with this name.");

    public static AppException OpenCases(int count) =>
        new(ErrorKind.Conflict, "asset.open_cases", $"This asset still has {count} open case(s): it cannot be retired yet.");

    public static AppException HasHistory() =>
        new(ErrorKind.Conflict, "asset.has_history", "This asset has a repair history: retire it instead of deleting it.");

    public static AppException ActionNotAllowed() =>
        new(ErrorKind.Conflict, "asset.action_not_allowed", "This action is not possible for this asset any more. Reload the page.");
}
