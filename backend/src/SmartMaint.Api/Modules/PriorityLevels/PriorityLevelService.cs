using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Core;
using SmartMaint.Api.Modules.Audit;
using SmartMaint.Api.Modules.PriorityLevels.Dtos;

namespace SmartMaint.Api.Modules.PriorityLevels;

internal sealed class PriorityLevelService(AppDbContext db, IAuditService audit) : IPriorityLevelService
{
    public async Task<IReadOnlyList<PriorityLevelResponse>> ListAsync(CancellationToken cancellationToken) =>
        (await db.PriorityLevels.AsNoTracking().ToListAsync(cancellationToken))
        .OrderBy(p => p.Code)
        .Select(ToResponse)
        .ToList();

    public async Task<PriorityLevelResponse> UpdateSlaAsync(long id, UpdateSlaRequest request, CancellationToken cancellationToken)
    {
        var level = await db.PriorityLevels.SingleOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw PriorityLevelErrors.NotFound();

        var before = ToResponse(level);
        level.SlaHours = request.SlaHours;
        var after = ToResponse(level);
        audit.Record(AuditTarget.PriorityLevel, AuditOperation.Update, before, after);
        await db.SaveChangesAsync(cancellationToken);
        return after;
    }

    private static PriorityLevelResponse ToResponse(PriorityLevel p) => new(p.Id, p.Code, p.Name, p.Description, p.SlaHours);
}
