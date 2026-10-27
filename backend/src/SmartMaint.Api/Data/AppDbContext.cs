using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Entities;
using SmartMaint.Api.Entities.Users;

namespace SmartMaint.Api.Data;

/// <summary>
/// Single DbContext for the whole system. Table/column names become snake_case automatically
/// (UseSnakeCaseNamingConvention in DatabaseServiceCollectionExtensions). Per-table mapping lives in Configurations/.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // created_at / updated_at also get a DB default so plain SQL inserts (Data.sql) work.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(AuditableEntity).IsAssignableFrom(t.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableEntity.CreatedAt)).HasDefaultValueSql("now()");
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableEntity.UpdatedAt)).HasDefaultValueSql("now()");
        }
    }
}
