using SmartMaint.Api.Common.Errors;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Core;
using SmartMaint.Api.Modules.Audit;
using SmartMaint.Api.Modules.Audit.Dtos;
using SmartMaint.Api.Modules.PriorityLevels;
using SmartMaint.Api.Modules.PriorityLevels.Dtos;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Core;

/// <summary>ADM-08 (audit log reading) and ADM-07 (SLA hours).</summary>
public sealed class AuditAndSlaTests : IDisposable
{
    private readonly FixedTimeProvider _clock = new(new DateTimeOffset(2026, 11, 2, 8, 0, 0, TimeSpan.Zero));
    private readonly FakeCurrentUser _me = new();
    private readonly AppDbContext _db;
    private readonly AuditService _audit;
    private readonly PriorityLevelService _levels;
    private readonly PriorityLevel _normal;

    public AuditAndSlaTests()
    {
        _db = TestDbContextFactory.Create(_clock);
        var admin = new Admin("root", "Root Admin", "hash", false);
        _normal = new PriorityLevel { Code = CasePriority.Normal, Name = "Normal", SlaHours = 24 };
        _db.AddRange(admin, _normal);
        _db.SaveChanges();
        _me.Id = admin.Id;
        _me.Role = UserRole.Admin;
        _audit = new AuditService(_db, _me, _clock);
        _levels = new PriorityLevelService(_db, _audit);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task UpdateSla_ChangesTheHoursAndAuditsOldAndNew()
    {
        var result = await _levels.UpdateSlaAsync(_normal.Id, new UpdateSlaRequest(12), default);

        Assert.Equal(12, result.SlaHours);
        var row = Assert.Single(_db.AuditLogs);
        Assert.Equal(AuditTarget.PriorityLevel, row.TargetType);
        Assert.Contains("\"slaHours\":24", row.OldValue);
        Assert.Contains("\"slaHours\":12", row.NewValue);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task UpdateSla_UnknownLevel_Throws()
    {
        var error = await Assert.ThrowsAsync<AppException>(() => _levels.UpdateSlaAsync(9999, new UpdateSlaRequest(12), default));

        Assert.Equal("priority_level.not_found", error.Code);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task ListAudit_FilteredByOperation_ReturnsOnlyThoseRows()
    {
        _audit.Record(AuditTarget.Account, AuditOperation.Create, null, new { Id = 1 });
        _audit.Record(AuditTarget.Account, AuditOperation.Deactivate, null, new { Id = 1 });
        await _db.SaveChangesAsync();

        var page = await _audit.ListAsync(new AuditLogQuery { Operation = AuditOperation.Deactivate }, default);

        Assert.Equal(AuditOperation.Deactivate, Assert.Single(page.Items).Operation);
        Assert.Equal("Root Admin", page.Items[0].AdminName);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task ListAudit_ToDate_IncludesTheWholeLastDay()
    {
        _clock.Now = new DateTimeOffset(2026, 11, 2, 23, 59, 59, TimeSpan.Zero);
        _audit.Record(AuditTarget.Account, AuditOperation.Update, null, null);
        await _db.SaveChangesAsync();

        var sameDay = await _audit.ListAsync(new AuditLogQuery { To = new DateOnly(2026, 11, 2) }, default);
        var dayBefore = await _audit.ListAsync(new AuditLogQuery { To = new DateOnly(2026, 11, 1) }, default);

        Assert.Equal(1, sameDay.TotalCount);
        Assert.Equal(0, dayBefore.TotalCount);
    }
}
