using SmartMaint.Api.Modules.Facilities.Dtos;

namespace SmartMaint.Api.Modules.Facilities;

/// <summary>
/// F1 (Huy) — AST-01 Facility tree: buildings, floors, rooms. Staff and managers read their own buildings; only the
/// Administrator creates, edits and deletes (Screen Catalog rule 7). A place with content is never deleted (BR-10).
/// Every change is in the audit log.
/// </summary>
public interface IFacilityService
{
    Task<IReadOnlyList<BuildingNodeResponse>> GetTreeAsync(CancellationToken cancellationToken);

    Task<BuildingNodeResponse> CreateBuildingAsync(BuildingRequest request, CancellationToken cancellationToken);
    Task<BuildingNodeResponse> UpdateBuildingAsync(long id, BuildingRequest request, CancellationToken cancellationToken);
    Task DeleteBuildingAsync(long id, CancellationToken cancellationToken);

    Task<FloorNodeResponse> CreateFloorAsync(FloorRequest request, CancellationToken cancellationToken);
    Task<FloorNodeResponse> UpdateFloorAsync(long id, FloorRequest request, CancellationToken cancellationToken);
    Task DeleteFloorAsync(long id, CancellationToken cancellationToken);

    Task<RoomNodeResponse> CreateRoomAsync(RoomRequest request, CancellationToken cancellationToken);
    Task<RoomNodeResponse> UpdateRoomAsync(long id, RoomRequest request, CancellationToken cancellationToken);
    Task DeleteRoomAsync(long id, CancellationToken cancellationToken);
}
