using SmartMaint.Api.Entities.Maintenance;

namespace SmartMaint.Api.Modules.Maintenance;

/// <summary>
/// The fixed schedule of F2 (Leader 02/11), per trigger since 19/11: due days are StartDate, StartDate + 1 period,
/// + 2 periods... until the trigger's end day. Late work never moves them (fit for inspections and fire safety). A work
/// order is made LeadDays before its due day.
/// </summary>
public static class MaintenanceSchedule
{
    public static DateOnly AddPeriods(DateOnly day, int periods, int frequency, FrequencyUnit unit) => unit switch
    {
        FrequencyUnit.Day => day.AddDays(periods * frequency),
        FrequencyUnit.Week => day.AddDays(7 * periods * frequency),
        FrequencyUnit.Month => day.AddMonths(periods * frequency),
        FrequencyUnit.Year => day.AddYears(periods * frequency),
        _ => throw new ArgumentOutOfRangeException(nameof(unit)),
    };

    /// <summary>The next due day of an asset after its last work order (or its start day), or null after its end day.</summary>
    public static DateOnly? NextDue(MaintenanceWorkOrderAsset asset, MaintenanceWorkOrderDetail trigger)
    {
        var next = asset.LastDueDate is { } last ? After(last, trigger) : asset.StartTime;
        return InRange(next, trigger);
    }

    /// <summary>
    /// The latest due day that is already "in reach" (due − lead days ≤ today), skipping older ones that were missed
    /// (e.g. while the plan was paused or past its end day): only one work order is made per asset per run.
    /// </summary>
    public static DateOnly? DueToGenerate(MaintenanceWorkOrderAsset asset, MaintenanceWorkOrderDetail trigger, DateOnly today)
    {
        DateOnly? found = null;
        for (var due = NextDue(asset, trigger); due is { } d && d.AddDays(-trigger.LeadDays) <= today; due = InRange(After(d, trigger), trigger))
        {
            found = d;
        }

        return found;
    }

    /// <summary>Expired = it has an end day and none of its (still used) assets has a due day left.</summary>
    public static bool Expired(MaintenanceWorkOrderDetail trigger) =>
        trigger.EndDate is not null && trigger.Assets.Where(a => !a.Asset.IsRetired).All(a => NextDue(a, trigger) is null);

    /// <summary>Length of one period in days (months = 30), used for "recently repaired" (G3: 10% of the period, P04).</summary>
    public static int PeriodDays(MaintenanceWorkOrderDetail trigger) => trigger.FrequencyType switch
    {
        FrequencyUnit.Day => trigger.Frequency,
        FrequencyUnit.Week => 7 * trigger.Frequency,
        FrequencyUnit.Month => 30 * trigger.Frequency,
        _ => 365 * trigger.Frequency,
    };

    private static DateOnly After(DateOnly day, MaintenanceWorkOrderDetail trigger) =>
        AddPeriods(day, 1, trigger.Frequency, trigger.FrequencyType);

    // The trigger's end day (the asset rows keep a copy for the code deployed before 19/11).
    private static DateOnly? InRange(DateOnly day, MaintenanceWorkOrderDetail trigger) =>
        trigger.EndDate is { } end && day > end ? null : day;
}
