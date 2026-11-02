using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Maintenance;
using SmartMaint.Api.Modules.Maintenance.Dtos;

namespace SmartMaint.Api.Modules.Maintenance;

/// <summary>
/// F2 checklist templates (ERD: CaseChecklists + ChecklistTasks), kept by the Administrator and the staff. A plan copies
/// the items into every work order it makes, so editing a template never changes work orders already made.
/// </summary>
public interface IChecklistService
{
    Task<IReadOnlyList<ChecklistResponse>> ListAsync(CancellationToken cancellationToken);

    Task<ChecklistResponse> SaveAsync(long? id, ChecklistRequest request, CancellationToken cancellationToken);

    /// <summary>Only when no plan uses it.</summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken);
}

internal sealed class ChecklistService(AppDbContext db) : IChecklistService
{
    public async Task<IReadOnlyList<ChecklistResponse>> ListAsync(CancellationToken cancellationToken) =>
        (await db.CaseChecklists.AsNoTracking()
            .Include(c => c.Category)
            .Include(c => c.Tasks)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken))
        .Select(c => new ChecklistResponse(
            c.Id, c.Name, c.Description, c.CategoryId, $"{c.Category.GroupName} · {c.Category.Name}",
            c.Tasks.OrderBy(t => t.Id).Select(t => new ChecklistItemResponse(t.Id, t.Name, t.Description)).ToList(),
            db.MaintenanceWorkOrderDetails.Count(d => d.ChecklistId == c.Id)))
        .ToList();

    public async Task<ChecklistResponse> SaveAsync(long? id, ChecklistRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await db.CaseChecklists.AnyAsync(c => c.Name.ToLower() == name.ToLower() && c.Id != (id ?? 0), cancellationToken))
        {
            throw MaintenanceErrors.ChecklistNameTaken();
        }

        if (!await db.CaseCategories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken))
        {
            throw MaintenanceErrors.UnknownCategory();
        }

        CaseChecklist checklist;
        if (id is { } existing)
        {
            checklist = await db.CaseChecklists.Include(c => c.Tasks).SingleOrDefaultAsync(c => c.Id == existing, cancellationToken)
                ?? throw MaintenanceErrors.ChecklistNotFound();
            db.ChecklistTasks.RemoveRange(checklist.Tasks);
        }
        else
        {
            checklist = new CaseChecklist();
            db.CaseChecklists.Add(checklist);
        }

        checklist.Name = name;
        checklist.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        checklist.CategoryId = request.CategoryId;
        checklist.Tasks = request.Tasks
            .Select(t => new ChecklistTask { Name = t.Name.Trim(), Description = string.IsNullOrWhiteSpace(t.Description) ? null : t.Description.Trim() })
            .ToList();
        await db.SaveChangesAsync(cancellationToken);

        return (await ListAsync(cancellationToken)).Single(c => c.Id == checklist.Id);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var checklist = await db.CaseChecklists.SingleOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw MaintenanceErrors.ChecklistNotFound();
        var plans = await db.MaintenanceWorkOrderDetails.CountAsync(d => d.ChecklistId == id, cancellationToken);
        if (plans > 0)
        {
            throw MaintenanceErrors.ChecklistInUse(plans);
        }

        db.CaseChecklists.Remove(checklist);
        await db.SaveChangesAsync(cancellationToken);
    }
}
