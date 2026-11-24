using SmartMaint.Api.Common.Errors;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>Every error the Cases module can return. Codes are part of the API contract.</summary>
internal static class CaseErrors
{
    // Also returned for a case the caller may not see: never reveal that it exists (B3).
    public static AppException NotFound() =>
        new(ErrorKind.NotFound, "case.not_found", "Case not found.");

    public static AppException InvalidTransition() =>
        new(ErrorKind.Conflict, "case.invalid_transition",
            "This action is no longer possible. The case may have been changed by someone else. Reload the page.");

    public static AppException UnknownServiceType() =>
        new(ErrorKind.BadRequest, "case.unknown_service_type", "Choose a service type.");

    public static AppException UnknownRoom() =>
        new(ErrorKind.BadRequest, "case.unknown_room", "Choose a room or area.");

    public static AppException FaultNotForAsset() =>
        new(ErrorKind.BadRequest, "case.fault_not_for_asset", "This fault category does not apply to the chosen asset.");

    public static AppException PlannedStartRequired() =>
        new(ErrorKind.BadRequest, "case.planned_start_required", "Choose the planned start.");

    public static AppException AssetNotInRoom() =>
        new(ErrorKind.BadRequest, "case.asset_not_in_room", "That asset is not in the chosen room, or it was retired.");

    public static AppException PhotoRequired(string serviceType) =>
        new(ErrorKind.BadRequest, "case.photo_required", $"\"{serviceType}\" needs at least one photo.");

    public static AppException TaskNotFound() =>
        new(ErrorKind.NotFound, "case.task_not_found", "This checklist item does not belong to the case.");

    public static AppException DueNotLater() =>
        new(ErrorKind.BadRequest, "case.due_not_later", "The new deadline must be later than the current one.");

    public static AppException InvalidTechnician() =>
        new(ErrorKind.BadRequest, "case.invalid_technician", "Choose an active technician.");

    public static AppException SurveyNotAllowed() =>
        new(ErrorKind.BadRequest, "case.survey_not_allowed",
            "Open a survey for an asset in use in one of your buildings that has no open survey yet.");

    public static AppException InvalidDuplicateTarget() =>
        new(ErrorKind.BadRequest, "case.invalid_duplicate_target",
            "Choose another open case of the same building as the one this request repeats.");

    public static AppException TaskAlreadyDone(string task) =>
        new(ErrorKind.BadRequest, "case.task_done", $"\"{task}\" already has a result and cannot be removed.");

    public static AppException TaskByThirdParty(string task) =>
        new(ErrorKind.Conflict, "case.task_third_party",
            $"\"{task}\" was done by an outside party; only the staff can change it.");

    public static AppException SameTechnician() =>
        new(ErrorKind.BadRequest, "case.same_technician", "This technician already has the case. Choose another one.");

    public static AppException CostNotYours() =>
        new(ErrorKind.Conflict, "case.cost_not_yours", "You can only edit the costs you added yourself.");

    public static AppException ReceiptRequired() =>
        new(ErrorKind.BadRequest, "case.receipt_required", "Add the receipt number or a photo of the receipt.");

    public static AppException CostNotFound() =>
        new(ErrorKind.NotFound, "case.cost_not_found", "This cost line does not belong to the case. Reload the page.");

    public static AppException InvalidFollower() =>
        new(ErrorKind.BadRequest, "case.invalid_follower", "Followers must be active customers of the case's building.");

    public static AppException StatusChangeNotAllowed() =>
        new(ErrorKind.Conflict, "case.status_change_not_allowed",
            "This status change is not possible for the case now. Reload the page.");
}
