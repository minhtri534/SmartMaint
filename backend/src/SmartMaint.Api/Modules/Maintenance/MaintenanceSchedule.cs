using SmartMaint.Api.Entities.Maintenance;

namespace SmartMaint.Api.Modules.Maintenance;

/// <summary>
/// The fixed schedule of F2 (Leader 02/11): due days are StartDate, StartDate + 1 period, + 2 periods... Late work never
/// moves them (fit for inspections and fire safety). A work order is made LeadDays before its due day.
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
    public static DateOnly? NextDue(MaintenanceWorkOrderAsset asset, MaintenanceTrigger plan)
    {
        var next = asset.LastDueDate is { } last ? AddPeriods(last, 1, plan.Frequency, plan.FrequencyType) : asset.StartTime;
        return asset.EndTime is { } end && next > end ? null : next;
    }

    /// <summary>
    /// The latest due day that is already "in reach" (due − lead days ≤ today), skipping older ones that were missed
    /// (e.g. while the plan was paused): only one work order is made per asset per run.
    /// </summary>
    public static DateOnly? DueToGenerate(MaintenanceWorkOrderAsset asset, MaintenanceTrigger plan, DateOnly today)
    {
        DateOnly? found = null;
        for (var due = NextDue(asset, plan); due is { } d && d.AddDays(-plan.LeadDays) <= today; due = After(d))
        {
            found = d;
        }

        return found;

        DateOnly? After(DateOnly d)
        {
            var next = AddPeriods(d, 1, plan.Frequency, plan.FrequencyType);
            return asset.EndTime is { } end && next > end ? null : next;
        }
    }

    /// <summary>Length of one period in days (months = 30), used for "recently repaired" (G3: 10% of the period, P04).</summary>
    public static int PeriodDays(MaintenanceTrigger plan) => plan.FrequencyType switch
    {
        FrequencyUnit.Day => plan.Frequency,
        FrequencyUnit.Week => 7 * plan.Frequency,
        FrequencyUnit.Month => 30 * plan.Frequency,
        _ => 365 * plan.Frequency,
    };
}
