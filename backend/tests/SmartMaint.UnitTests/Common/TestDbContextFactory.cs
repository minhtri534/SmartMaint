using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartMaint.Api.Data;
using SmartMaint.Api.Data.Interceptors;

namespace SmartMaint.UnitTests.Common;

/// <summary>A fresh, isolated in-memory AppDbContext per test.</summary>
public static class TestDbContextFactory
{
    public static AppDbContext Create(TimeProvider? clock = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new TimestampInterceptor(clock ?? TimeProvider.System))
            // The in-memory provider has no transactions; services that use one (change role) still run.
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }
}
