using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Data.Conventions;
using SmartMaint.Api.Entities;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Data;

/// <summary>
/// Single DbContext for the whole system. Table/column names become snake_case automatically
/// (UseSnakeCaseNamingConvention in DatabaseServiceCollectionExtensions). Per-table mapping lives in Configurations/.
/// Tables follow Hùng's ERD v5 (Drive, logical page); differences are listed in docs/database/ERD_v5_Thay_Doi_Khi_Code.md.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Accounts (core): one table per actor sharing the account id
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<TechnicianSpecialty> TechnicianSpecialties => Set<TechnicianSpecialty>();

    // Buildings and assets (F1)
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<AssetType> AssetTypes => Set<AssetType>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<StaffBuildingAssignment> StaffBuildingAssignments => Set<StaffBuildingAssignment>();
    public DbSet<ManagerBuildingAssignment> ManagerBuildingAssignments => Set<ManagerBuildingAssignment>();

    // Cases (F3, shared by F2 / F4 / F5)
    public DbSet<PriorityLevel> PriorityLevels => Set<PriorityLevel>();
    public DbSet<CaseCategory> CaseCategories => Set<CaseCategory>();
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<CaseImage> CaseImages => Set<CaseImage>();
    public DbSet<CaseStatusHistory> CaseStatusHistory => Set<CaseStatusHistory>();
    public DbSet<TechnicianRejection> TechnicianRejections => Set<TechnicianRejection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ERD lookup tables: one row per enum member, written by the migration.
        modelBuilder.HasLookupTable<UserRole>("roles");
        modelBuilder.HasLookupTable<CaseStatus>("case_statuses");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // created_at / updated_at also get a DB default so plain SQL inserts (Data.sql) work.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => t.BaseType is null && typeof(AuditableEntity).IsAssignableFrom(t.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableEntity.CreatedAt)).HasDefaultValueSql("now()");
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableEntity.UpdatedAt)).HasDefaultValueSql("now()");
        }
    }
}
