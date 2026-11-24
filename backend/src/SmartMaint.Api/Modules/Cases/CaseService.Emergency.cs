using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>
/// F4 emergency (docs/flows/F4_Khan_Cap, Part 6 decisions of 02/11): the steps an emergency case has on top of the F3
/// arrows. Same state machine and same <see cref="ActAsync"/> as F3, so history, scope and notifications stay in one place.
/// The timers (no acknowledgement, technician not accepting) are in <see cref="EmergencyWatch"/>.
/// </summary>
internal sealed partial class CaseService
{
    private static readonly Dictionary<EmergencyLogKind, string> LogLabels = new()
    {
        [EmergencyLogKind.ThirdParty] = "Third party called",
        [EmergencyLogKind.Workaround] = "Temporary measure",
        [EmergencyLogKind.CustomerUpdate] = "Update for the customer",
        [EmergencyLogKind.Other] = "Note",
    };

    /// <summary>
    /// The case is an emergency from <paramref name="now"/>: 24/7 deadline = now + the emergency SLA hours (Admin setting).
    /// <paramref name="acknowledged"/>: a staff member made it an emergency themselves, so nobody needs to be chased.
    /// </summary>
    internal static void EnterEmergency(Case c, PriorityLevel emergency, DateTimeOffset now, bool acknowledged)
    {
        c.PriorityLevelId = emergency.Id;
        c.Priority = emergency;
        c.EmergencyAt = now;
        c.DueAt = now.AddHours(emergency.SlaHours);
        c.SlaWarnedAt = null;
        c.SlaOverdueNotifiedAt = null;
        if (acknowledged)
        {
            c.AcknowledgedAt ??= now;
        }
    }

    // ----- actions ----------------------------------------------------------------------------------------------

    /// <summary>F4 step ②: "I have it". Stops the escalation; this staff member is now in charge of the case.</summary>
    public Task<CaseDetailResponse> AcknowledgeAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.Acknowledge, async (c, now) =>
        {
            c.AcknowledgedAt = now;
            c.Staff = await db.Staff.SingleAsync(s => s.Id == currentUser.Id, cancellationToken);
            var minutes = (int)Math.Round((now - (c.EmergencyAt ?? c.SubmittedAt)).TotalMinutes);
            return $"Emergency acknowledged after {minutes} min.";
        }, cancellationToken);

    /// <summary>N3 / N2: a line of the emergency log (third party called, temporary measure, news for the customer).</summary>
    public Task<CaseDetailResponse> AddLogAsync(long id, EmergencyLogRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.AddLog, (c, _) =>
        {
            var line = $"{LogLabels[request.Kind]}: {request.Note.Trim()}";
            // The technician is the contact person for the outside party; the customer reads measures and news.
            long[] to = request.Kind switch
            {
                EmergencyLogKind.ThirdParty => c.AssignedTechnicianId is { } t ? [t] : [],
                EmergencyLogKind.Workaround or EmergencyLogKind.CustomerUpdate => c.RequestingCustomerId is { } r ? [r] : [],
                _ => [],
            };
            notifications.Send(to, $"{Case.CodeOf(c.Id)} \"{c.Title}\": {line}");
            return Task.FromResult<string?>(line);
        }, cancellationToken);

    /// <summary>F4 step ⑫ (K08): optional post-incident report after closing; may be rewritten.</summary>
    public Task<CaseDetailResponse> WriteIncidentReportAsync(
        long id, IncidentReportRequest request, CancellationToken cancellationToken) =>
        ActAsync(id, CaseAction.WriteIncidentReport, (c, now) =>
        {
            var first = c.IncidentReportedAt is null;
            c.IncidentDamage = request.Damage.Trim();
            c.IncidentLessons = request.Lessons.Trim();
            c.IncidentFollowUp = request.FollowUp;
            c.IncidentReportedAt = now;
            return Task.FromResult<string?>(first ? "Post-incident report written." : "Post-incident report updated.");
        }, cancellationToken);

    // ----- notifications ----------------------------------------------------------------------------------------

    /// <summary>The primary staff of the building; every staff member when nobody is primary.</summary>
    internal static async Task<List<long>> PrimaryStaffAsync(Data.AppDbContext db, long buildingId, bool primary, CancellationToken cancellationToken)
    {
        var staff = await db.StaffBuildingAssignments
            .Where(a => a.BuildingId == buildingId && a.Staff.Status == UserStatus.Active)
            .Select(a => new { a.StaffId, a.IsPrimary })
            .ToListAsync(cancellationToken);
        var wanted = staff.Where(a => a.IsPrimary == primary).Select(a => a.StaffId).ToList();
        return wanted.Count > 0 || !primary ? wanted : staff.Select(a => a.StaffId).ToList();
    }

    /// <summary>Who hears about the F4 steps (called from NotifyAsync for the actions it does not list; AddLog notifies itself).</summary>
    private void NotifyEmergency(Case c, CaseAction action, List<long> staffOfBuilding)
    {
        var code = Case.CodeOf(c.Id);
        var customer = CustomersOf(c);
        switch (action)
        {
            case CaseAction.Acknowledge:
                notifications.Send(customer, $"{code}: {c.Staff?.FullName ?? "The building staff"} is handling your emergency.");
                break;
        }
    }

    /// <summary>
    /// Closing a main case tells the customers of the merged reports (N5). An emergency fixed after its deadline asks
    /// the staff for a post-incident report (K08).
    /// </summary>
    private async Task NotifyEmergencyClosedAsync(Case c, List<long> staffOfBuilding, CancellationToken cancellationToken)
    {
        var code = Case.CodeOf(c.Id);
        var merged = await db.Cases
            .Where(x => x.MergedIntoCaseId == c.Id && x.RequestingCustomerId != null)
            .Select(x => new { x.Id, CustomerId = x.RequestingCustomerId!.Value })
            .ToListAsync(cancellationToken);
        foreach (var m in merged)
        {
            notifications.Send([m.CustomerId], $"{Case.CodeOf(m.Id)}: the problem you reported was fixed under {code}.");
        }

        if (CaseMapper.IsEmergencyFlow(c) && c.DueAt is { } due && c.CompletedAt > due)
        {
            notifications.Send(c.StaffId is { } s ? [s] : staffOfBuilding,
                $"{code} \"{c.Title}\" was fixed after its deadline. Please write a post-incident report.");
        }
    }
}
