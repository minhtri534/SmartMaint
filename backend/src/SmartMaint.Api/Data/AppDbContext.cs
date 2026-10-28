using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Entities;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Users;

namespace SmartMaint.Api.Data;

/// <summary>
/// Single DbContext for the whole system. Table/column names become snake_case automatically
/// (UseSnakeCaseNamingConvention in DatabaseServiceCollectionExtensions). Per-table mapping lives in Configurations/.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    // Buildings (minimal, F1 extends)
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<StaffBuildingAssignment> StaffBuildingAssignments => Set<StaffBuildingAssignment>();
    public DbSet<ManagerBuildingAssignment> ManagerBuildingAssignments => Set<ManagerBuildingAssignment>();

    // Cases (F3)
    public DbSet<PriorityLevel> PriorityLevels => Set<PriorityLevel>();
    public DbSet<CaseCategory> CaseCategories => Set<CaseCategory>();
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<CaseImage> CaseImages => Set<CaseImage>();
    public DbSet<CaseStatusChange> CaseStatusHistory => Set<CaseStatusChange>();
    public DbSet<CaseAssignment> CaseAssignments => Set<CaseAssignment>();

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
