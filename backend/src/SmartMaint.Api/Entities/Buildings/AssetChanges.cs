using SmartMaint.Api.Entities.Accounts;

namespace SmartMaint.Api.Entities.Buildings;

// NOT IN ERD v5 (meeting note 18/11, decision 7, tree version of 24/11): buildings, floors, rooms and assets change only
// through a list a staff member writes on the building tree (their own buildings, plus new ones) and a manager of those
// buildings approves. Deleting a building, floor or room only marks it (DeletedAt); deleting an asset retires it. The
// Administrator only reads them. Both tables are listed in docs/database/ERD_v5_Thay_Doi_Khi_Code.md.

/// <summary>Where a change list is. Stored as text + CHECK.</summary>
public enum AssetChangeListStatus
{
    /// <summary>The staff member is still writing it; nobody else acts on it.</summary>
    Draft,

    /// <summary>Sent: waits for a manager of the building.</summary>
    Pending,

    /// <summary>Approved: every line was applied at that moment. Read only.</summary>
    Approved,

    /// <summary>Rejected with a reason: the staff member edits it and sends it again.</summary>
    Rejected,
}

/// <summary>What one line of a list changes. Stored as text + CHECK.</summary>
public enum AssetChangeKind
{
    AddBuilding,
    EditBuilding,
    DeleteBuilding,
    AddFloor,
    RenameFloor,
    DeleteFloor,
    AddRoom,
    RenameRoom,
    DeleteRoom,
    AddAsset,
    EditAsset,
    RetireAsset,
    DeleteAsset,
}

/// <summary>NOT IN ERD v5: a list of changes to buildings, floors, rooms and assets.</summary>
public class AssetChangeList : AuditableEntity
{
    /// <summary>Lists written before 24/11 belonged to one building; a tree list may touch several, so it is empty.</summary>
    public long? BuildingId { get; set; }
    public Building? Building { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; }

    public AssetChangeListStatus Status { get; set; }

    /// <summary>The staff member who wrote it.</summary>
    public long StaffId { get; set; }
    public Staff Staff { get; set; } = null!;

    /// <summary>The manager who approved or rejected it last.</summary>
    public long? ReviewerId { get; set; }
    public Manager? Reviewer { get; set; }

    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? RejectReason { get; set; }

    public List<AssetChangeItem> Items { get; set; } = [];
}

/// <summary>
/// NOT IN ERD v5: one line of a change list. Which columns are used depends on <see cref="Kind"/>: the target (floor,
/// room or asset) for a change to something that exists, the new values for an add or an edit. A new room can sit on a
/// floor added by the same list, a new asset in a room added by the same list: <see cref="ParentKey"/> names that line's
/// <see cref="Key"/>.
/// </summary>
public class AssetChangeItem : AuditableEntity
{
    public long ListId { get; set; }
    public AssetChangeList List { get; set; } = null!;

    /// <summary>Order on screen (and the line number in messages), from 1.</summary>
    public int Position { get; set; }

    public AssetChangeKind Kind { get; set; }

    /// <summary>The line's own short key inside its list, e.g. "n3" (written by the web form).</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The key of the AddBuilding / AddFloor / AddRoom line of the same list this line goes in, instead of
    /// BuildingId / FloorId / RoomId.
    /// </summary>
    public string? ParentKey { get; set; }

    /// <summary>The building changed (Edit / DeleteBuilding) or the building of a new floor. After approval: the one added.</summary>
    public long? BuildingId { get; set; }
    public Building? Building { get; set; }

    /// <summary>The floor changed (Rename / DeleteFloor) or the floor of a new room. After approval: the floor added.</summary>
    public long? FloorId { get; set; }
    public Floor? Floor { get; set; }

    /// <summary>The room changed (Rename / DeleteRoom) or the room of a new / edited asset. After approval: the room added.</summary>
    public long? RoomId { get; set; }
    public Room? Room { get; set; }

    /// <summary>The asset changed (Edit / Retire / DeleteAsset). After approval of an AddAsset: the asset added.</summary>
    public long? AssetId { get; set; }
    public Asset? Asset { get; set; }

    /// <summary>The new name: the building name, the floor number, the room number or the asset name.</summary>
    public string? Name { get; set; }

    /// <summary>The new address of an added or edited building.</summary>
    public string? Address { get; set; }

    public long? TypeId { get; set; }
    public AssetType? Type { get; set; }

    public long? ManufacturerId { get; set; }
    public Manufacturer? Manufacturer { get; set; }

    public bool? HasCopyright { get; set; }
    public DateOnly? WarrantyEndDate { get; set; }

    /// <summary>What the line changes, as it read when it was saved ("Floor 5 · 501 · AC 501"): kept after a delete.</summary>
    public string? TargetLabel { get; set; }
}
