using Microsoft.EntityFrameworkCore;
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
            .Options;
        return new AppDbContext(options);
    }
}
