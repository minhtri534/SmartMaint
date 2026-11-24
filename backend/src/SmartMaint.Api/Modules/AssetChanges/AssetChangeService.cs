using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Errors;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.AssetChanges.Dtos;
using SmartMaint.Api.Modules.Assets;
using SmartMaint.Api.Modules.Assets.Dtos;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.Api.Modules.Facilities.Dtos;
using SmartMaint.Api.Modules.Notifications;

namespace SmartMaint.Api.Modules.AssetChanges;

/// <summary>
/// F1, meeting note 18/11 (decision 7), tree version of 24/11: buildings, floors, rooms and assets change only through a
/// change list. A staff member edits the building tree of their own buildings (and adds new buildings); every edit is a
/// line of their draft. They send it; a manager who looks after every building it touches approves it (every line is
/// applied at once, through the facility and asset services, so their rules stay the same) or rejects it with a reason.
/// Deleting a building, floor or room only marks it; deleting an asset retires it.
/// </summary>
public interface IAssetChangeService
{
    /// <summary>Staff: their own lists. Manager: the sent lists that touch their buildings (or only add new ones).</summary>
    Task<IReadOnlyList<ChangeListSummaryResponse>> ListAsync(CancellationToken cancellationToken);

    Task<ChangeListDetailResponse> GetAsync(long id, CancellationToken cancellationToken);

    /// <summary>Staff: a new draft (may start empty: the tree saves after the first edit).</summary>
    Task<ChangeListDetailResponse> CreateAsync(ChangeListRequest request, CancellationToken cancellationToken);

    /// <summary>Staff: saves a draft or a rejected list again (its lines are replaced).</summary>
    Task<ChangeListDetailResponse> UpdateAsync(long id, ChangeListRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(long id, CancellationToken cancellationToken);

    /// <summary>Staff: sends it to the managers. Refused while it is empty or a line has a problem.</summary>
    Task<ChangeListDetailResponse> SubmitAsync(long id, CancellationToken cancellationToken);

    /// <summary>Manager: applies every line, or none when one of them fails.</summary>
    Task<ChangeListDetailResponse> ApproveAsync(long id, CancellationToken cancellationToken);

    Task<ChangeListDetailResponse> RejectAsync(long id, RejectChangeListRequest request, CancellationToken cancellationToken);
}

internal sealed class AssetChangeService(
    AppDbContext db,
    ICurrentUser currentUser,
    IBuildingScope scope,
    IAssetService assets,
    IFacilityService facilities,
    INotificationService notifications,
    TimeProvider clock) : IAssetChangeService
{
    private static readonly CaseStatus[] Finished = [CaseStatus.Closed, CaseStatus.Rejected, CaseStatus.Cancelled];

    /// <summary>The order lines are checked and applied in: what others build on first, deletes last (inside out).</summary>
    private static readonly AssetChangeKind[] ApplyOrder =
    [
        AssetChangeKind.AddBuilding, AssetChangeKind.EditBuilding, AssetChangeKind.AddFloor, AssetChangeKind.RenameFloor,
        AssetChangeKind.AddRoom, AssetChangeKind.RenameRoom, AssetChangeKind.AddAsset, AssetChangeKind.EditAsset,
        AssetChangeKind.RetireAsset, AssetChangeKind.DeleteAsset, AssetChangeKind.DeleteRoom, AssetChangeKind.DeleteFloor,
        AssetChangeKind.DeleteBuilding,
    ];

    // ----- reads ------------------------------------------------------------------------------------------------

    public async Task<IReadOnlyList<ChangeListSummaryResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var lists = await VisibleAsync(cancellationToken);
        var names = await BuildingNamesAsync(lists, cancellationToken);
        return lists
            .OrderBy(l => l.Status == AssetChangeListStatus.Pending ? 0 : 1)
            .ThenByDescending(l => l.UpdatedAt)
            .Select(l => Summary(l, names[l.Id]))
            .ToList();
    }

    public async Task<ChangeListDetailResponse> GetAsync(long id, CancellationToken cancellationToken)
    {
        var list = (await VisibleAsync(cancellationToken)).SingleOrDefault(l => l.Id == id) ?? throw AssetChangeErrors.NotFound();
        var problems = list.Status == AssetChangeListStatus.Approved
            ? new Dictionary<int, string>()
            : (await SimulateAsync(list, cancellationToken)).Problems;
        var types = await db.AssetTypes.AsNoTracking().ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);
        var makers = await db.Manufacturers.AsNoTracking().ToDictionaryAsync(m => m.Id, m => m.Name, cancellationToken);
        var names = await BuildingNamesAsync([list], cancellationToken);
        var at = await PlacesAsync(list, cancellationToken);

        var items = list.Items.OrderBy(i => i.Position).Select(i => new ChangeItemResponse(
            i.Id, i.Position, i.Kind, i.Key, i.ParentKey, i.BuildingId, i.FloorId, i.RoomId, i.AssetId, i.Name, i.Address,
            i.TypeId, i.TypeId is { } t ? types.GetValueOrDefault(t) : null,
            i.ManufacturerId, i.ManufacturerId is { } m ? makers.GetValueOrDefault(m) : null,
            i.HasCopyright, i.WarrantyEndDate, i.TargetLabel, at.GetValueOrDefault(i.Id), problems.GetValueOrDefault(i.Position))).ToList();
        var s = Summary(list, names[list.Id]);
        return new ChangeListDetailResponse(
            s.Id, s.Code, s.Title, s.Status, s.BuildingNames, s.ItemCount, s.Staff, s.Reviewer, s.UpdatedAt,
            s.SubmittedAt, s.ReviewedAt, list.Note, list.RejectReason, items, await AllowedActionsAsync(list, cancellationToken));
    }

    // ----- staff ------------------------------------------------------------------------------------------------

    public async Task<ChangeListDetailResponse> CreateAsync(ChangeListRequest request, CancellationToken cancellationToken)
    {
        var list = new AssetChangeList { StaffId = currentUser.Id, Status = AssetChangeListStatus.Draft };
        await ApplyRequestAsync(list, request, cancellationToken);
        db.AssetChangeLists.Add(list);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(list.Id, cancellationToken);
    }

    public async Task<ChangeListDetailResponse> UpdateAsync(long id, ChangeListRequest request, CancellationToken cancellationToken)
    {
        var list = await FindAsync(id, ChangeListAction.Edit, cancellationToken);
        db.AssetChangeItems.RemoveRange(list.Items);
        list.Items = [];
        // Two saves: the old lines leave before the new ones (same keys) come in, for the unique (list, key) index.
        await db.SaveChangesAsync(cancellationToken);
        await ApplyRequestAsync(list, request, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var list = await FindAsync(id, ChangeListAction.Delete, cancellationToken);
        db.AssetChangeLists.Remove(list);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ChangeListDetailResponse> SubmitAsync(long id, CancellationToken cancellationToken)
    {
        var list = await FindAsync(id, ChangeListAction.Submit, cancellationToken);
        if (list.Items.Count == 0) throw AssetChangeErrors.Empty();
        var result = await SimulateAsync(list, cancellationToken);
        ThrowOnProblem(result);
        list.Status = AssetChangeListStatus.Pending;
        list.SubmittedAt = clock.GetUtcNow();
        list.RejectReason = null;

        // The managers of the buildings it touches; a list that only adds buildings goes to every manager.
        var managers = await db.ManagerBuildingAssignments
            .Where(a => (result.Touched.Count == 0 || result.Touched.Contains(a.BuildingId)) && a.Manager.Status == UserStatus.Active)
            .Select(a => a.ManagerId).Distinct().ToListAsync(cancellationToken);
        notifications.Send(managers, $"Change list {CodeOf(list.Id)} \"{list.Title}\" ({list.Items.Count} changes) waits for your approval.");
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    // ----- manager ----------------------------------------------------------------------------------------------

    public async Task<ChangeListDetailResponse> ApproveAsync(long id, CancellationToken cancellationToken)
    {
        var list = await FindAsync(id, ChangeListAction.Approve, cancellationToken);
        ThrowOnProblem(await SimulateAsync(list, cancellationToken));

        // One transaction: every line or none (the in-memory test database has no transactions).
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(cancellationToken)
            : null;
        await ApplyLinesAsync(list, cancellationToken);
        list.Status = AssetChangeListStatus.Approved;
        list.ReviewerId = currentUser.Id;
        list.ReviewedAt = clock.GetUtcNow();
        notifications.Send([list.StaffId], $"Change list {CodeOf(list.Id)} \"{list.Title}\" was approved: the changes are in place.");
        await db.SaveChangesAsync(cancellationToken);
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        return await GetAsync(id, cancellationToken);
    }

    public async Task<ChangeListDetailResponse> RejectAsync(long id, RejectChangeListRequest request, CancellationToken cancellationToken)
    {
        var list = await FindAsync(id, ChangeListAction.Reject, cancellationToken);
        list.Status = AssetChangeListStatus.Rejected;
        list.ReviewerId = currentUser.Id;
        list.ReviewedAt = clock.GetUtcNow();
        list.RejectReason = request.Reason.Trim();
        notifications.Send([list.StaffId], $"Change list {CodeOf(list.Id)} \"{list.Title}\" was rejected: {list.RejectReason}");
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    // ----- applying ---------------------------------------------------------------------------------------------

    /// <summary>
    /// Runs each line through the facility / asset service (same rules as before), in <see cref="ApplyOrder"/>. A line
    /// that fails stops everything with its line number. The new ids are written back on the lines; a new building gets
    /// the staff member who proposed it (as the primary staff) and the approving manager.
    /// </summary>
    private async Task ApplyLinesAsync(AssetChangeList list, CancellationToken cancellationToken)
    {
        var newIds = new Dictionary<string, long>();
        long Parent(long? id, string? key) => id ?? newIds[key!];
        foreach (var item in Ordered(list.Items))
        {
            try
            {
                switch (item.Kind)
                {
                    case AssetChangeKind.AddBuilding:
                        // Made here, not by the facility service: its reply reads the tree of the caller's buildings, and
                        // the new building is not one of them until the assignments below. The name was checked above.
                        var building = new Building { Name = item.Name!.Trim(), Address = item.Address };
                        db.Buildings.Add(building);
                        await db.SaveChangesAsync(cancellationToken);
                        item.BuildingId = newIds[item.Key] = building.Id;
                        db.StaffBuildingAssignments.Add(new StaffBuildingAssignment { StaffId = list.StaffId, BuildingId = building.Id, IsPrimary = true });
                        db.ManagerBuildingAssignments.Add(new ManagerBuildingAssignment { ManagerId = currentUser.Id, BuildingId = building.Id });
                        await db.SaveChangesAsync(cancellationToken);
                        break;
                    case AssetChangeKind.EditBuilding:
                        await facilities.UpdateBuildingAsync(item.BuildingId!.Value, new BuildingRequest(item.Name!, item.Address), cancellationToken);
                        break;
                    case AssetChangeKind.AddFloor:
                        var buildingId = Parent(item.BuildingId, item.ParentKey);
                        var floor = await facilities.CreateFloorAsync(new FloorRequest(buildingId, item.Name!), cancellationToken);
                        item.BuildingId = buildingId;
                        item.FloorId = newIds[item.Key] = floor.Id;
                        break;
                    case AssetChangeKind.RenameFloor:
                        var floorBuilding = await db.Floors.Where(f => f.Id == item.FloorId).Select(f => f.BuildingId).SingleAsync(cancellationToken);
                        await facilities.UpdateFloorAsync(item.FloorId!.Value, new FloorRequest(floorBuilding, item.Name!), cancellationToken);
                        break;
                    case AssetChangeKind.AddRoom:
                        var floorId = Parent(item.FloorId, item.ParentKey);
                        var room = await facilities.CreateRoomAsync(new RoomRequest(floorId, item.Name!), cancellationToken);
                        item.FloorId = floorId;
                        item.RoomId = newIds[item.Key] = room.Id;
                        break;
                    case AssetChangeKind.RenameRoom:
                        var roomFloor = await db.Rooms.Where(r => r.Id == item.RoomId).Select(r => r.FloorId).SingleAsync(cancellationToken);
                        await facilities.UpdateRoomAsync(item.RoomId!.Value, new RoomRequest(roomFloor, item.Name!), cancellationToken);
                        break;
                    case AssetChangeKind.AddAsset:
                        item.RoomId = Parent(item.RoomId, item.ParentKey);
                        item.AssetId = (await assets.CreateAsync(AssetRequestOf(item), cancellationToken)).Id;
                        break;
                    case AssetChangeKind.EditAsset:
                        item.RoomId = Parent(item.RoomId, item.ParentKey);
                        await assets.UpdateAsync(item.AssetId!.Value, AssetRequestOf(item), cancellationToken);
                        break;
                    case AssetChangeKind.RetireAsset:
                        await assets.RetireAsync(item.AssetId!.Value, cancellationToken);
                        break;
                    case AssetChangeKind.DeleteAsset:
                        await assets.DeleteAsync(item.AssetId!.Value, cancellationToken);
                        break;
                    case AssetChangeKind.DeleteRoom:
                        await facilities.DeleteRoomAsync(item.RoomId!.Value, cancellationToken);
                        break;
                    case AssetChangeKind.DeleteFloor:
                        await facilities.DeleteFloorAsync(item.FloorId!.Value, cancellationToken);
                        break;
                    case AssetChangeKind.DeleteBuilding:
                        await facilities.DeleteBuildingAsync(item.BuildingId!.Value, cancellationToken);
                        break;
                }
            }
            catch (AppException e)
            {
                throw AssetChangeErrors.HasProblems(item.Position, e.Message);
            }
        }
    }

    private static IEnumerable<AssetChangeItem> Ordered(IEnumerable<AssetChangeItem> items) =>
        items.OrderBy(i => Array.IndexOf(ApplyOrder, i.Kind)).ThenBy(i => i.Position);

    private static AssetRequest AssetRequestOf(AssetChangeItem i) =>
        new(i.Name!.Trim(), i.RoomId!.Value, i.TypeId!.Value, i.ManufacturerId, i.HasCopyright ?? false, i.WarrantyEndDate);

    // ----- checks -----------------------------------------------------------------------------------------------

    private static void ThrowOnProblem(Simulation result)
    {
        if (result.Problems.Count > 0)
        {
            var first = result.Problems.OrderBy(p => p.Key).First();
            throw AssetChangeErrors.HasProblems(first.Key, first.Value);
        }
    }

    /// <summary>What a check of a list found: the problem of each line (by line number) and the existing buildings touched.</summary>
    private sealed record Simulation(Dictionary<int, string> Problems, HashSet<long> Touched);

    /// <summary>
    /// A building, floor, room or asset of the simulated tree. Key "b12" / "f3" / "r7" / "a9", or a line's key. Gone =
    /// deleted or retired. A row of the database keeps its name even when gone (the unique indexes count it).
    /// </summary>
    private sealed class Node(string key, string? parent, string name, bool gone, bool inDatabase = true)
    {
        public string Key { get; } = key;
        public string? Parent { get; set; } = parent;
        public string Name { get; set; } = name;
        public bool Gone { get; set; } = gone;
        public bool InDatabase { get; } = inDatabase;
    }

    /// <summary>
    /// Applies the list to a copy of the writer's buildings in memory, in the order it would really be applied, and notes
    /// what would stop each line. Run on every read, before sending and again before approving: the buildings may have
    /// changed in between.
    /// </summary>
    private async Task<Simulation> SimulateAsync(AssetChangeList list, CancellationToken cancellationToken)
    {
        var mine = await db.StaffBuildingAssignments.Where(a => a.StaffId == list.StaffId)
            .Select(a => a.BuildingId).ToListAsync(cancellationToken);
        var buildings = (await db.Buildings.AsNoTracking()
                .Select(b => new { b.Id, b.Name, Gone = b.DeletedAt != null }).ToListAsync(cancellationToken))
            .ToDictionary(b => $"b{b.Id}", b => new Node($"b{b.Id}", null, b.Name, b.Gone));
        var floors = (await db.Floors.AsNoTracking().Where(f => mine.Contains(f.BuildingId))
                .Select(f => new { f.Id, f.BuildingId, f.FloorNumber, Gone = f.DeletedAt != null }).ToListAsync(cancellationToken))
            .ToDictionary(f => $"f{f.Id}", f => new Node($"f{f.Id}", $"b{f.BuildingId}", f.FloorNumber, f.Gone));
        var rooms = (await db.Rooms.AsNoTracking().Where(r => mine.Contains(r.Floor.BuildingId))
                .Select(r => new { r.Id, r.FloorId, r.RoomNumber, Gone = r.DeletedAt != null }).ToListAsync(cancellationToken))
            .ToDictionary(r => $"r{r.Id}", r => new Node($"r{r.Id}", $"f{r.FloorId}", r.RoomNumber, r.Gone));
        var targetAssets = list.Items.Where(i => i.AssetId is not null).Select(i => i.AssetId!.Value).ToList();
        var assetNodes = (await db.Assets.AsNoTracking().Where(a => mine.Contains(a.Room.Floor.BuildingId))
                .Select(a => new { a.Id, a.RoomId, a.Name, Retired = a.RetiredAt != null }).ToListAsync(cancellationToken))
            .ToDictionary(a => $"a{a.Id}", a => new Node($"a{a.Id}", $"r{a.RoomId}", a.Name, a.Retired));
        var openByAsset = (await db.Cases.AsNoTracking()
                .Where(c => c.AssetId != null && targetAssets.Contains(c.AssetId.Value) && !Finished.Contains(c.Status))
                .Select(c => c.AssetId!.Value).ToListAsync(cancellationToken)).ToHashSet();
        var usedAssets = (await db.Cases.AsNoTracking().Where(c => c.AssetId != null && targetAssets.Contains(c.AssetId.Value))
                .Select(c => c.AssetId!.Value).Distinct().ToListAsync(cancellationToken)).ToHashSet();
        var openRooms = (await db.Cases.AsNoTracking().Where(c => mine.Contains(c.Room.Floor.BuildingId) && !Finished.Contains(c.Status))
                .Select(c => new { c.RoomId, c.Room.Floor.BuildingId }).ToListAsync(cancellationToken));
        var openRoomKeys = openRooms.Select(r => $"r{r.RoomId}").ToHashSet();
        var openBuildingKeys = openRooms.Select(r => $"b{r.BuildingId}").ToHashSet();
        var typeIds = (await db.AssetTypes.AsNoTracking().Select(t => t.Id).ToListAsync(cancellationToken)).ToHashSet();
        var makerIds = (await db.Manufacturers.AsNoTracking().Select(m => m.Id).ToListAsync(cancellationToken)).ToHashSet();

        var problems = new Dictionary<int, string>();
        var touched = new HashSet<long>();
        var changed = new HashSet<string>();

        // An existing node the writer may change, or the line's problem.
        string? Target(Dictionary<string, Node> nodes, string key, string what)
        {
            if (!nodes.TryGetValue(key, out var node) || node.Gone || (key.StartsWith('b') && !mine.Contains(long.Parse(key[1..]))))
                return $"This {what} is not in your buildings any more.";
            return changed.Add(key) ? null : "An earlier line already changes the same thing.";
        }

        // The parent of a new / moved node: an existing one ("b12") or a new one added by a line of this list.
        string? ParentKeyOf(string prefix, long? id, string? lineKey) => id is { } x ? $"{prefix}{x}" : lineKey;

        string? Parent(Dictionary<string, Node> nodes, string? key, string what) =>
            key is null || !nodes.TryGetValue(key, out var node) || node.Gone
                ? $"Choose the {what}."
                : key.StartsWith('b') && !mine.Contains(long.Parse(key[1..]))
                    ? "This building is not one of yours."
                    : null;

        // A name is taken by a node in use, or by any row of the database (a deleted one keeps its name).
        bool Taken(Dictionary<string, Node> nodes, string? parent, string name, string? except) =>
            nodes.Values.Any(n => (n.InDatabase || !n.Gone) && n.Parent == parent && n.Key != except
                                  && string.Equals(n.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));

        void Touch(string? buildingKey)
        {
            if (buildingKey is not null && buildingKey.StartsWith('b')) touched.Add(long.Parse(buildingKey[1..]));
        }

        string? BuildingOfFloor(string? floorKey) => floorKey is not null && floors.TryGetValue(floorKey, out var f) ? f.Parent : null;
        string? BuildingOfRoom(string? roomKey) => roomKey is not null && rooms.TryGetValue(roomKey, out var r) ? BuildingOfFloor(r.Parent) : null;

        foreach (var item in Ordered(list.Items))
        {
            var name = item.Name?.Trim() ?? string.Empty;
            string? problem = null;
            switch (item.Kind)
            {
                case AssetChangeKind.AddBuilding:
                    problem = Taken(buildings, null, name, null) ? $"A building \"{name}\" already exists." : null;
                    if (problem is null) buildings[item.Key] = new Node(item.Key, null, name, false, inDatabase: false);
                    break;
                case AssetChangeKind.EditBuilding:
                    var editKey = $"b{item.BuildingId}";
                    problem = Target(buildings, editKey, "building")
                              ?? (Taken(buildings, null, name, editKey) ? $"A building \"{name}\" already exists." : null);
                    if (problem is null) buildings[editKey].Name = name;
                    Touch(editKey);
                    break;
                case AssetChangeKind.DeleteBuilding:
                    var dropKey = $"b{item.BuildingId}";
                    problem = Target(buildings, dropKey, "building")
                              ?? (floors.Values.Any(f => !f.Gone && f.Parent == dropKey) ? "The building still has floors: delete them in this list first."
                                  : openBuildingKeys.Contains(dropKey) ? "The building has open cases." : null);
                    if (problem is null) buildings[dropKey].Gone = true;
                    Touch(dropKey);
                    break;
                case AssetChangeKind.AddFloor:
                    var floorParent = ParentKeyOf("b", item.BuildingId, item.ParentKey);
                    problem = Parent(buildings, floorParent, "building of the new floor")
                              ?? (Taken(floors, floorParent, name, null) ? $"The building already has a floor \"{name}\"." : null);
                    if (problem is null) floors[item.Key] = new Node(item.Key, floorParent, name, false, inDatabase: false);
                    Touch(floorParent);
                    break;
                case AssetChangeKind.RenameFloor:
                    var renameFloor = $"f{item.FloorId}";
                    problem = Target(floors, renameFloor, "floor")
                              ?? (Taken(floors, floors[renameFloor].Parent, name, renameFloor) ? $"The building already has a floor \"{name}\"." : null);
                    if (problem is null) floors[renameFloor].Name = name;
                    Touch(BuildingOfFloor(renameFloor));
                    break;
                case AssetChangeKind.DeleteFloor:
                    var dropFloor = $"f{item.FloorId}";
                    problem = Target(floors, dropFloor, "floor")
                              ?? (rooms.Values.Any(r => !r.Gone && r.Parent == dropFloor) ? "The floor still has rooms: delete them in this list first." : null);
                    if (problem is null) floors[dropFloor].Gone = true;
                    Touch(BuildingOfFloor(dropFloor));
                    break;
                case AssetChangeKind.AddRoom:
                    var roomParent = ParentKeyOf("f", item.FloorId, item.ParentKey);
                    problem = Parent(floors, roomParent, "floor of the new room")
                              ?? (Taken(rooms, roomParent, name, null) ? $"The floor already has a room \"{name}\"." : null);
                    if (problem is null) rooms[item.Key] = new Node(item.Key, roomParent, name, false, inDatabase: false);
                    Touch(BuildingOfFloor(roomParent));
                    break;
                case AssetChangeKind.RenameRoom:
                    var renameRoom = $"r{item.RoomId}";
                    problem = Target(rooms, renameRoom, "room")
                              ?? (Taken(rooms, rooms[renameRoom].Parent, name, renameRoom) ? $"The floor already has a room \"{name}\"." : null);
                    if (problem is null) rooms[renameRoom].Name = name;
                    Touch(BuildingOfRoom(renameRoom));
                    break;
                case AssetChangeKind.DeleteRoom:
                    var dropRoom = $"r{item.RoomId}";
                    problem = Target(rooms, dropRoom, "room")
                              ?? (assetNodes.Values.Any(a => !a.Gone && a.Parent == dropRoom) ? "The room still has assets: move or delete them in this list first."
                                  : openRoomKeys.Contains(dropRoom) ? "The room has open cases." : null);
                    if (problem is null) rooms[dropRoom].Gone = true;
                    Touch(BuildingOfRoom(dropRoom));
                    break;
                case AssetChangeKind.AddAsset or AssetChangeKind.EditAsset:
                    var assetKey = item.Kind == AssetChangeKind.EditAsset ? $"a{item.AssetId}" : item.Key;
                    var assetRoom = ParentKeyOf("r", item.RoomId, item.ParentKey);
                    problem = (item.Kind == AssetChangeKind.EditAsset ? Target(assetNodes, assetKey, "asset") : null)
                              ?? Parent(rooms, assetRoom, "room of the asset")
                              ?? (!typeIds.Contains(item.TypeId ?? 0) ? "Choose the asset type."
                                  : item.ManufacturerId is { } maker && !makerIds.Contains(maker) ? "This manufacturer does not exist."
                                  : Taken(assetNodes, assetRoom, name, assetKey) ? $"The room already has an asset \"{name}\"." : null);
                    if (item.Kind == AssetChangeKind.EditAsset && assetNodes.TryGetValue(assetKey, out var before))
                        Touch(BuildingOfRoom(before.Parent)); // a move touches the building it leaves too
                    if (problem is null)
                    {
                        if (item.Kind == AssetChangeKind.AddAsset) assetNodes[assetKey] = new Node(assetKey, assetRoom, name, false, inDatabase: false);
                        else (assetNodes[assetKey].Parent, assetNodes[assetKey].Name) = (assetRoom, name);
                    }

                    Touch(BuildingOfRoom(assetRoom));
                    break;
                case AssetChangeKind.RetireAsset or AssetChangeKind.DeleteAsset:
                    var gone = $"a{item.AssetId}";
                    problem = Target(assetNodes, gone, "asset")
                              ?? (item.Kind == AssetChangeKind.RetireAsset
                                  ? openByAsset.Contains(item.AssetId!.Value) ? "The asset still has open cases." : null
                                  : usedAssets.Contains(item.AssetId!.Value) ? "The asset has a repair history: delete (retire) it instead." : null);
                    if (assetNodes.TryGetValue(gone, out var node)) Touch(BuildingOfRoom(node.Parent));
                    if (problem is null) assetNodes[gone].Gone = true;
                    break;
            }

            if (problem is not null) problems[item.Position] = problem;
        }

        return new Simulation(problems, touched);
    }

    // ----- helpers ----------------------------------------------------------------------------------------------

    /// <summary>
    /// Who may do what: the staff who wrote it edit / send / delete a draft or rejected list; a manager who looks after
    /// every building it touches approves or rejects a sent one.
    /// </summary>
    private async Task<IReadOnlyList<ChangeListAction>> AllowedActionsAsync(AssetChangeList list, CancellationToken cancellationToken)
    {
        if (currentUser.Role == UserRole.Staff && list.StaffId == currentUser.Id
            && list.Status is AssetChangeListStatus.Draft or AssetChangeListStatus.Rejected)
        {
            return [ChangeListAction.Edit, ChangeListAction.Submit, ChangeListAction.Delete];
        }

        if (currentUser.Role != UserRole.Manager || list.Status != AssetChangeListStatus.Pending) return [];
        var mine = await scope.GetBuildingIdsAsync(cancellationToken) ?? [];
        var touched = (await SimulateAsync(list, cancellationToken)).Touched;
        return touched.All(mine.Contains) ? [ChangeListAction.Approve, ChangeListAction.Reject] : [];
    }

    /// <summary>
    /// Staff: their own lists. Manager: every sent list that touches one of their buildings, or touches no existing
    /// building (it only adds new ones). Small numbers: filtered in memory.
    /// </summary>
    private async Task<List<AssetChangeList>> VisibleAsync(CancellationToken cancellationToken)
    {
        var query = db.AssetChangeLists.AsNoTracking()
            .Include(l => l.Staff).Include(l => l.Reviewer).Include(l => l.Items);
        if (currentUser.Role == UserRole.Staff)
        {
            return await query.Where(l => l.StaffId == currentUser.Id).ToListAsync(cancellationToken);
        }

        var mine = await scope.GetBuildingIdsAsync(cancellationToken) ?? [];
        var sent = await query.Where(l => l.Status != AssetChangeListStatus.Draft).ToListAsync(cancellationToken);
        var visible = new List<AssetChangeList>();
        foreach (var list in sent)
        {
            var touched = await TouchedAsync(list, cancellationToken);
            if (touched.Count == 0 || touched.Any(mine.Contains)) visible.Add(list);
        }

        return visible;
    }

    /// <summary>The existing buildings a list touches, read from its lines' targets (also after approval).</summary>
    private async Task<HashSet<long>> TouchedAsync(AssetChangeList list, CancellationToken cancellationToken)
    {
        var floorIds = list.Items.Where(i => i.FloorId is not null).Select(i => i.FloorId!.Value).ToList();
        var roomIds = list.Items.Where(i => i.RoomId is not null).Select(i => i.RoomId!.Value).ToList();
        var assetIds = list.Items.Where(i => i.AssetId is not null).Select(i => i.AssetId!.Value).ToList();
        var ids = list.Items.Where(i => i.BuildingId is not null && i.Kind != AssetChangeKind.AddBuilding)
            .Select(i => i.BuildingId!.Value).ToHashSet();
        ids.UnionWith(await db.Floors.Where(f => floorIds.Contains(f.Id)).Select(f => f.BuildingId).ToListAsync(cancellationToken));
        ids.UnionWith(await db.Rooms.Where(r => roomIds.Contains(r.Id)).Select(r => r.Floor.BuildingId).ToListAsync(cancellationToken));
        ids.UnionWith(await db.Assets.Where(a => assetIds.Contains(a.Id)).Select(a => a.Room.Floor.BuildingId).ToListAsync(cancellationToken));
        // A building the list added itself is not "touched" for the approval rule.
        ids.ExceptWith(list.Items.Where(i => i.Kind == AssetChangeKind.AddBuilding && i.BuildingId is not null).Select(i => i.BuildingId!.Value));
        return ids;
    }

    /// <summary>Where each line's existing target (or the existing place a new one goes in) is now, by line id.</summary>
    private async Task<Dictionary<long, ChangePlaceRef>> PlacesAsync(AssetChangeList list, CancellationToken cancellationToken)
    {
        var floorIds = list.Items.Where(i => i.FloorId is not null).Select(i => i.FloorId!.Value).ToList();
        var roomIds = list.Items.Where(i => i.RoomId is not null).Select(i => i.RoomId!.Value).ToList();
        var assetIds = list.Items.Where(i => i.AssetId is not null).Select(i => i.AssetId!.Value).ToList();
        var floors = await db.Floors.AsNoTracking().Where(f => floorIds.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, f => f.BuildingId, cancellationToken);
        var rooms = (await db.Rooms.AsNoTracking().Where(r => roomIds.Contains(r.Id))
                .Select(r => new { r.Id, r.FloorId, r.Floor.BuildingId }).ToListAsync(cancellationToken))
            .ToDictionary(r => r.Id, r => new ChangePlaceRef(r.BuildingId, r.FloorId, r.Id));
        var assets = (await db.Assets.AsNoTracking().Where(a => assetIds.Contains(a.Id))
                .Select(a => new { a.Id, a.RoomId, a.Room.FloorId, a.Room.Floor.BuildingId }).ToListAsync(cancellationToken))
            .ToDictionary(a => a.Id, a => new ChangePlaceRef(a.BuildingId, a.FloorId, a.RoomId));

        var result = new Dictionary<long, ChangePlaceRef>();
        foreach (var i in list.Items)
        {
            ChangePlaceRef? place = i.Kind switch
            {
                // an asset sits where it is now (a move also marks where it goes: RoomId / ParentKey)
                AssetChangeKind.EditAsset or AssetChangeKind.RetireAsset or AssetChangeKind.DeleteAsset =>
                    assets.GetValueOrDefault(i.AssetId ?? 0),
                AssetChangeKind.AddAsset or AssetChangeKind.RenameRoom or AssetChangeKind.DeleteRoom =>
                    i.RoomId is { } r ? rooms.GetValueOrDefault(r) : null,
                AssetChangeKind.AddRoom or AssetChangeKind.RenameFloor or AssetChangeKind.DeleteFloor =>
                    i.FloorId is { } f && floors.TryGetValue(f, out var b) ? new ChangePlaceRef(b, f, null) : null,
                AssetChangeKind.AddFloor or AssetChangeKind.EditBuilding or AssetChangeKind.DeleteBuilding =>
                    i.BuildingId is { } building ? new ChangePlaceRef(building, null, null) : null,
                _ => null,
            };
            if (place is not null) result[i.Id] = place;
        }

        return result;
    }

    /// <summary>The names of the buildings each list touches, new ones (added by a line) included.</summary>
    private async Task<Dictionary<long, IReadOnlyList<string>>> BuildingNamesAsync(
        IReadOnlyList<AssetChangeList> lists, CancellationToken cancellationToken)
    {
        var result = new Dictionary<long, IReadOnlyList<string>>();
        var allNames = await db.Buildings.AsNoTracking().ToDictionaryAsync(b => b.Id, b => b.Name, cancellationToken);
        foreach (var list in lists)
        {
            var touched = await TouchedAsync(list, cancellationToken);
            result[list.Id] = touched.Select(id => allNames.GetValueOrDefault(id, "?"))
                .Concat(list.Items.Where(i => i.Kind == AssetChangeKind.AddBuilding).Select(i => i.Name ?? "?"))
                .Distinct().Order().ToList();
        }

        return result;
    }

    private async Task<AssetChangeList> FindAsync(long id, ChangeListAction action, CancellationToken cancellationToken)
    {
        if ((await VisibleAsync(cancellationToken)).All(l => l.Id != id)) throw AssetChangeErrors.NotFound();
        var list = await db.AssetChangeLists
            .Include(l => l.Staff).Include(l => l.Reviewer).Include(l => l.Items)
            .SingleAsync(l => l.Id == id, cancellationToken);
        return (await AllowedActionsAsync(list, cancellationToken)).Contains(action) ? list : throw AssetChangeErrors.ActionNotAllowed();
    }

    /// <summary>Copies the request onto the list and labels each line's target as it reads now.</summary>
    private async Task ApplyRequestAsync(AssetChangeList list, ChangeListRequest request, CancellationToken cancellationToken)
    {
        list.Title = request.Title.Trim();
        list.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        var buildingIds = request.Items.Where(i => i.BuildingId is not null).Select(i => i.BuildingId!.Value).ToList();
        var floorIds = request.Items.Where(i => i.FloorId is not null).Select(i => i.FloorId!.Value).ToList();
        var roomIds = request.Items.Where(i => i.RoomId is not null).Select(i => i.RoomId!.Value).ToList();
        var assetIds = request.Items.Where(i => i.AssetId is not null).Select(i => i.AssetId!.Value).ToList();
        var buildingLabels = await db.Buildings.AsNoTracking().Where(b => buildingIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Name, cancellationToken);
        var floorLabels = (await db.Floors.AsNoTracking().Where(f => floorIds.Contains(f.Id))
                .Select(f => new { f.Id, Building = f.Building.Name, f.FloorNumber }).ToListAsync(cancellationToken))
            .ToDictionary(f => f.Id, f => $"{f.Building} · Floor {f.FloorNumber}");
        var roomLabels = (await db.Rooms.AsNoTracking().Where(r => roomIds.Contains(r.Id))
                .Select(r => new { r.Id, Building = r.Floor.Building.Name, r.Floor.FloorNumber, r.RoomNumber }).ToListAsync(cancellationToken))
            .ToDictionary(r => r.Id, r => $"{r.Building} · Floor {r.FloorNumber} · {r.RoomNumber}");
        var assetLabels = (await db.Assets.AsNoTracking().Where(a => assetIds.Contains(a.Id))
                .Select(a => new { a.Id, a.Name, Building = a.Room.Floor.Building.Name, a.Room.Floor.FloorNumber, a.Room.RoomNumber })
                .ToListAsync(cancellationToken))
            .ToDictionary(a => a.Id, a => $"{a.Name} ({a.Building} · Floor {a.FloorNumber} · {a.RoomNumber})");

        list.Items = request.Items.Select((r, index) => new AssetChangeItem
        {
            Position = index + 1,
            Kind = r.Kind,
            Key = r.Key,
            ParentKey = string.IsNullOrEmpty(r.ParentKey) ? null : r.ParentKey,
            BuildingId = r.BuildingId,
            FloorId = r.FloorId,
            RoomId = r.RoomId,
            AssetId = r.AssetId,
            Name = r.Name?.Trim(),
            Address = string.IsNullOrWhiteSpace(r.Address) ? null : r.Address.Trim(),
            TypeId = r.TypeId,
            ManufacturerId = r.ManufacturerId,
            HasCopyright = r.HasCopyright,
            WarrantyEndDate = r.WarrantyEndDate,
            TargetLabel = r.Kind switch
            {
                AssetChangeKind.EditBuilding or AssetChangeKind.DeleteBuilding => buildingLabels.GetValueOrDefault(r.BuildingId ?? 0),
                AssetChangeKind.AddFloor => r.BuildingId is { } b ? buildingLabels.GetValueOrDefault(b) : null,
                AssetChangeKind.RenameFloor or AssetChangeKind.DeleteFloor or AssetChangeKind.AddRoom =>
                    r.FloorId is { } f ? floorLabels.GetValueOrDefault(f) : null,
                AssetChangeKind.RenameRoom or AssetChangeKind.DeleteRoom or AssetChangeKind.AddAsset =>
                    r.RoomId is { } room ? roomLabels.GetValueOrDefault(room) : null,
                AssetChangeKind.EditAsset or AssetChangeKind.RetireAsset or AssetChangeKind.DeleteAsset =>
                    assetLabels.GetValueOrDefault(r.AssetId ?? 0),
                _ => null,
            },
        }).ToList();
    }

    public static string CodeOf(long id) => $"CL-{id:D6}";

    private static ChangeListSummaryResponse Summary(AssetChangeList l, IReadOnlyList<string> buildingNames) => new(
        l.Id, CodeOf(l.Id), l.Title, l.Status, buildingNames, l.Items.Count,
        new ChangePersonRef(l.Staff.Id, l.Staff.FullName),
        l.Reviewer is null ? null : new ChangePersonRef(l.Reviewer.Id, l.Reviewer.FullName),
        l.UpdatedAt, l.SubmittedAt, l.ReviewedAt);
}
