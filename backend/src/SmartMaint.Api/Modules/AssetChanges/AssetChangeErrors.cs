using SmartMaint.Api.Common.Errors;

namespace SmartMaint.Api.Modules.AssetChanges;

/// <summary>Every error the AssetChanges module can return. Codes are part of the API contract.</summary>
internal static class AssetChangeErrors
{
    // Also returned for another staff member's list, or one that touches none of a manager's buildings.
    public static AppException NotFound() =>
        new(ErrorKind.NotFound, "asset_change.not_found", "Change list not found.");

    public static AppException Empty() =>
        new(ErrorKind.BadRequest, "asset_change.empty", "Make at least one change before sending the list.");

    public static AppException HasProblems(int line, string problem) =>
        new(ErrorKind.Conflict, "asset_change.has_problems", $"Line {line}: {problem}");

    public static AppException ActionNotAllowed() =>
        new(ErrorKind.Conflict, "asset_change.action_not_allowed",
            "This action is not possible for this list any more. Reload the page.");
}
