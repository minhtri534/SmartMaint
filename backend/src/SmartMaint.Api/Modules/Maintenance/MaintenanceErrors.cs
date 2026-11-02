using SmartMaint.Api.Common.Errors;

namespace SmartMaint.Api.Modules.Maintenance;

/// <summary>Every error the Maintenance module (F2) can return. Codes are part of the API contract.</summary>
internal static class MaintenanceErrors
{
    public static AppException ChecklistNotFound() =>
        new(ErrorKind.NotFound, "maintenance.checklist_not_found", "Checklist not found.");

    public static AppException ChecklistNameTaken() =>
        new(ErrorKind.Conflict, "maintenance.checklist_name_taken", "Another checklist already has this name.");

    public static AppException ChecklistInUse(int plans) =>
        new(ErrorKind.Conflict, "maintenance.checklist_in_use", $"{plans} plan(s) use this checklist: it cannot be deleted.");

    public static AppException UnknownCategory() =>
        new(ErrorKind.BadRequest, "maintenance.unknown_category", "Choose the service type of the checklist.");

    // Also returned for a plan outside the caller's buildings (B3).
    public static AppException PlanNotFound() =>
        new(ErrorKind.NotFound, "maintenance.plan_not_found", "Maintenance plan not found.");

    public static AppException UnknownAsset() =>
        new(ErrorKind.BadRequest, "maintenance.unknown_asset", "One of the assets does not exist or is retired.");

    public static AppException AssetsInSeveralBuildings() =>
        new(ErrorKind.BadRequest, "maintenance.several_buildings",
            "All assets of a plan must be in the same building, one of the buildings you look after.");

    public static AppException StartInPast() =>
        new(ErrorKind.BadRequest, "maintenance.start_in_past", "The first due day cannot be in the past.");

    public static AppException UnknownTechnician() =>
        new(ErrorKind.BadRequest, "maintenance.unknown_technician", "One of the technicians is not an active technician.");

    public static AppException ActionNotAllowed() =>
        new(ErrorKind.Conflict, "maintenance.action_not_allowed",
            "This action is not possible for this plan any more. Reload the page.");
}
