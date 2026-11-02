using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Facilities;
using SmartMaint.Api.Modules.Facilities.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>F1 AST-01: the facility tree. Staff / Manager read their buildings; the Administrator changes it.</summary>
[ApiController]
[Route("api/facilities")]
[Produces("application/json")]
[AuthorizeRoles(UserRole.Staff, UserRole.Manager, UserRole.Admin)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
public sealed class FacilitiesController(IFacilityService facilities) : ControllerBase
{
    [HttpGet("tree")]
    public Task<IReadOnlyList<BuildingNodeResponse>> Tree(CancellationToken cancellationToken) =>
        facilities.GetTreeAsync(cancellationToken);

    [HttpPost("buildings")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task<BuildingNodeResponse> CreateBuilding(BuildingRequest request, CancellationToken cancellationToken) =>
        facilities.CreateBuildingAsync(request, cancellationToken);

    [HttpPut("buildings/{id:long}")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task<BuildingNodeResponse> UpdateBuilding(long id, BuildingRequest request, CancellationToken cancellationToken) =>
        facilities.UpdateBuildingAsync(id, request, cancellationToken);

    [HttpDelete("buildings/{id:long}")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task DeleteBuilding(long id, CancellationToken cancellationToken) =>
        facilities.DeleteBuildingAsync(id, cancellationToken);

    [HttpPost("floors")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task<FloorNodeResponse> CreateFloor(FloorRequest request, CancellationToken cancellationToken) =>
        facilities.CreateFloorAsync(request, cancellationToken);

    [HttpPut("floors/{id:long}")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task<FloorNodeResponse> UpdateFloor(long id, FloorRequest request, CancellationToken cancellationToken) =>
        facilities.UpdateFloorAsync(id, request, cancellationToken);

    [HttpDelete("floors/{id:long}")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task DeleteFloor(long id, CancellationToken cancellationToken) =>
        facilities.DeleteFloorAsync(id, cancellationToken);

    [HttpPost("rooms")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task<RoomNodeResponse> CreateRoom(RoomRequest request, CancellationToken cancellationToken) =>
        facilities.CreateRoomAsync(request, cancellationToken);

    [HttpPut("rooms/{id:long}")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task<RoomNodeResponse> UpdateRoom(long id, RoomRequest request, CancellationToken cancellationToken) =>
        facilities.UpdateRoomAsync(id, request, cancellationToken);

    [HttpDelete("rooms/{id:long}")]
    [AuthorizeRoles(UserRole.Admin)]
    public Task DeleteRoom(long id, CancellationToken cancellationToken) =>
        facilities.DeleteRoomAsync(id, cancellationToken);
}
