using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Catalog.Dtos;

namespace SmartMaint.Api.Modules.Catalog;

/// <summary>
/// The technicians a staff member can choose, with their specialties and open jobs. For one case they also say who fits
/// it: the ones who fit come first, then the least busy (meeting note 18/11).
/// </summary>
internal static class TechnicianOptions
{
    /// <summary>Statuses where a technician named on the case still has the job (Approved = not accepted yet).</summary>
    private static readonly CaseStatus[] OpenStatuses =
    [
        CaseStatus.Approved, CaseStatus.Accepted, CaseStatus.InProgress, CaseStatus.OnHold, CaseStatus.Reopened,
    ];

    /// <summary>Other words for the usual specialties; any other specialty is matched by the words of its own name.</summary>
    private static readonly Dictionary<string, string[]> SameTrade = new(StringComparer.OrdinalIgnoreCase)
    {
        ["HVAC"] = ["air", "conditioner", "conditioning", "ventilation", "cooling", "ac"],
        ["Electrical"] = ["electric", "power", "light", "socket", "switch", "breaker", "distribution", "elec"],
        ["Plumbing"] = ["plumb", "water", "pipe", "drain", "toilet", "leak", "tap", "heater"],
        ["Elevator"] = ["elevators", "lift", "passenger"],
    };

    /// <summary>Words of a specialty that say nothing about the trade.</summary>
    private static readonly string[] Vague = ["general", "repair", "maintenance", "and"];

    public static async Task<List<TechnicianOptionResponse>> LoadAsync(
        AppDbContext db, long staffId, long? caseId, CancellationToken cancellationToken)
    {
        var technicians = await db.Technicians.AsNoTracking()
            .Where(t => t.Status == UserStatus.Active)
            .Select(t => new
            {
                t.Id,
                t.FullName,
                Specialties = t.Specialties.Select(s => s.Specialty.Name).OrderBy(n => n).ToList(),
            })
            .ToListAsync(cancellationToken);

        var myBuildings = await db.StaffBuildingAssignments
            .Where(a => a.StaffId == staffId)
            .Select(a => a.BuildingId)
            .ToListAsync(cancellationToken);
        var jobs = await db.Cases.AsNoTracking()
            .Where(c => c.AssignedTechnicianId != null && OpenStatuses.Contains(c.Status) && c.Id != caseId)
            .Select(c => new
            {
                TechnicianId = c.AssignedTechnicianId!.Value,
                c.Id,
                c.Title,
                c.Status,
                Urgency = c.Priority.Code,
                c.PlannedStartAt,
                c.PlannedEndAt,
                c.DueAt,
                Mine = myBuildings.Contains(c.Room.Floor.BuildingId),
            })
            .ToListAsync(cancellationToken);

        IReadOnlySet<string> work = caseId is { } id ? await WorkWordsAsync(db, id, myBuildings, cancellationToken) : new HashSet<string>();
        return technicians
            .Select(t =>
            {
                var held = jobs.Where(j => j.TechnicianId == t.Id)
                    .OrderBy(j => j.PlannedStartAt ?? j.DueAt ?? DateTimeOffset.MaxValue)
                    .Select(j => new TechnicianJobResponse(
                        j.Id, Case.CodeOf(j.Id), j.Mine ? j.Title : null, j.Status, j.Urgency, j.PlannedStartAt, j.PlannedEndAt, j.DueAt))
                    .ToList();
                return new TechnicianOptionResponse(t.Id, t.FullName, held.Count, t.Specialties, held, Fits(t.Specialties, work));
            })
            .OrderByDescending(t => t.Fits).ThenBy(t => t.OpenCases).ThenBy(t => t.FullName)
            .ToList();
    }

    /// <summary>Does one of the specialties name the trade of this work?</summary>
    internal static bool Fits(IEnumerable<string> specialties, IReadOnlySet<string> work) =>
        specialties.Any(s => Words(s).Except(Vague).Concat(SameTrade.GetValueOrDefault(s, [])).Any(work.Contains));

    /// <summary>What the case is about, as lower-case words. A case outside the caller's buildings says nothing.</summary>
    private static async Task<IReadOnlySet<string>> WorkWordsAsync(
        AppDbContext db, long caseId, List<long> buildings, CancellationToken cancellationToken)
    {
        var parts = await db.Cases.AsNoTracking()
            .Where(c => c.Id == caseId && buildings.Contains(c.Room.Floor.BuildingId))
            .Select(c => new
            {
                c.Category.GroupName,
                c.Category.Name,
                Type = c.Asset == null ? null : c.Asset.Type.Name,
                Category = c.Asset == null ? null : c.Asset.Type.Category.CategoryName,
                Code = c.Asset == null ? null : c.Asset.Type.Category.Code,
            })
            .SingleOrDefaultAsync(cancellationToken);
        return parts is null
            ? new HashSet<string>()
            : Words($"{parts.GroupName} {parts.Name} {parts.Type} {parts.Category} {parts.Code}").ToHashSet();
    }

    private static IEnumerable<string> Words(string text) =>
        text.ToLowerInvariant().Split([' ', ',', '.', '-', '/', '(', ')'], StringSplitOptions.RemoveEmptyEntries);
}
