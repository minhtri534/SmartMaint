using SmartMaint.Api.Common.Errors;

namespace SmartMaint.Api.Modules.CatalogAdmin;

/// <summary>Every error the CatalogAdmin module can return. Codes are part of the API contract.</summary>
internal static class CatalogAdminErrors
{
    public static AppException NotFound() =>
        new(ErrorKind.NotFound, "catalog.not_found", "This item does not exist.");

    public static AppException Duplicate() =>
        new(ErrorKind.Conflict, "catalog.duplicate", "An item with this code or name already exists.");

    public static AppException InUse(int count) =>
        new(ErrorKind.Conflict, "catalog.in_use", $"{count} record(s) still use this item: it cannot be deleted.");

    public static AppException UnknownCategory() =>
        new(ErrorKind.BadRequest, "catalog.unknown_category", "Choose an asset category.");
}
