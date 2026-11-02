using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;

namespace SmartMaint.Api.Modules.Facilities;

/// <summary>
/// Which buildings the caller may see in F1 screens (B3): staff and managers only their assigned buildings,
/// the Administrator all of them (null = no limit). Other roles see none.
/// </summary>
public interface IBuildingScope
{
    Task<IReadOnlyList<long>?> GetBuildingIdsAsync(CancellationToken cancellationToken);
}

internal sealed class BuildingScope(AppDbContext db, ICurrentUser currentUser) : IBuildingScope
{
    public async Task<IReadOnlyList<long>?> GetBuildingIdsAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        return currentUser.Role switch
        {
            UserRole.Admin => null,
            UserRole.Staff => await db.StaffBuildingAssignments
                .Where(a => a.StaffId == userId).Select(a => a.BuildingId).ToListAsync(cancellationToken),
            UserRole.Manager => await db.ManagerBuildingAssignments
                .Where(a => a.ManagerId == userId).Select(a => a.BuildingId).ToListAsync(cancellationToken),
            _ => [],
        };
    }
}
