using SmartMaint.Api.Entities.Buildings;

namespace SmartMaint.Api.Modules.AssetChanges.Dtos;

// Meeting note 18/11 (decision 7), tree version of 24/11: a staff member edits buildings, floors, rooms and assets on the
// building tree; every edit is a line of their draft list (saved after each edit). A manager of the buildings it touches
// approves it (all lines applied at once) or rejects it with a reason. Names match
// frontend/src/features/asset-changes/types.ts.

/// <summary>
/// One line. Which fields count depends on <see cref="Kind"/>:
/// AddBuilding (Name, Address) · EditBuilding (BuildingId, Name, Address) · DeleteBuilding (BuildingId) ·
/// AddFloor (BuildingId or ParentKey of an AddBuilding line, Name = floor number) · RenameFloor (FloorId, Name) ·
/// DeleteFloor (FloorId) · AddRoom (FloorId or ParentKey of an AddFloor line, Name) · RenameRoom (RoomId, Name) ·
/// DeleteRoom (RoomId) · AddAsset (RoomId or ParentKey of an AddRoom line, Name, TypeId, ManufacturerId, HasCopyright,
/// WarrantyEndDate) · EditAsset (AssetId + the same fields; another room = a move) · RetireAsset (AssetId: the tree's
/// "Delete", kept in the history) · DeleteAsset (AssetId: only one never used in a case).
/// </summary>
public sealed record ChangeItemRequest(
    AssetChangeKind Kind,
    string Key,
    string? ParentKey,
    long? BuildingId,
    long? FloorId,
    long? RoomId,
    long? AssetId,
    string? Name,
    string? Address,
    long? TypeId,
    long? ManufacturerId,
    bool? HasCopyright,
    DateOnly? WarrantyEndDate);

/// <summary>A new list, or a whole list saved again (its lines are replaced): the tree saves after every edit.</summary>
public sealed record ChangeListRequest(string Title, string? Note, IReadOnlyList<ChangeItemRequest> Items);

public sealed record RejectChangeListRequest(string Reason);

public enum ChangeListAction
{
    Edit,
    Submit,
    Delete,
    Approve,
    Reject,
}

public sealed record ChangePersonRef(long Id, string FullName);

public sealed record ChangeItemResponse(
    long Id,
    int Position,
    AssetChangeKind Kind,
    string Key,
    string? ParentKey,
    long? BuildingId,
    long? FloorId,
    long? RoomId,
    long? AssetId,
    string? Name,
    string? Address,
    long? TypeId,
    string? TypeName,
    long? ManufacturerId,
    string? ManufacturerName,
    bool? HasCopyright,
    DateOnly? WarrantyEndDate,
    string? TargetLabel,
    ChangePlaceRef? At,
    string? Problem);

/// <summary>
/// Where the existing thing a line works on is now (the tree marks every level above it): the building, floor and room
/// of a changed asset / room / floor, or of the place a new one goes in. Null for a line inside a new building.
/// </summary>
public sealed record ChangePlaceRef(long BuildingId, long? FloorId, long? RoomId);

/// <summary>One row of the list page. <see cref="BuildingNames"/>: the buildings it touches, new ones included.</summary>
public record ChangeListSummaryResponse(
    long Id,
    string Code,
    string Title,
    AssetChangeListStatus Status,
    IReadOnlyList<string> BuildingNames,
    int ItemCount,
    ChangePersonRef Staff,
    ChangePersonRef? Reviewer,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset? ReviewedAt);

/// <summary>
/// One list. A line's Problem says why it cannot be applied now (checked again on every read until approved): a list
/// with a problem cannot be sent or approved.
/// </summary>
public sealed record ChangeListDetailResponse(
    long Id,
    string Code,
    string Title,
    AssetChangeListStatus Status,
    IReadOnlyList<string> BuildingNames,
    int ItemCount,
    ChangePersonRef Staff,
    ChangePersonRef? Reviewer,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset? ReviewedAt,
    string? Note,
    string? RejectReason,
    IReadOnlyList<ChangeItemResponse> Items,
    IReadOnlyList<ChangeListAction> AllowedActions)
    : ChangeListSummaryResponse(
        Id, Code, Title, Status, BuildingNames, ItemCount, Staff, Reviewer, UpdatedAt, SubmittedAt, ReviewedAt);
