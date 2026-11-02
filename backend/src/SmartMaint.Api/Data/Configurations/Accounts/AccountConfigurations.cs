using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartMaint.Api.Data.Conventions;
using SmartMaint.Api.Entities.Accounts;

namespace SmartMaint.Api.Data.Configurations.Accounts;

/// <summary>
/// accounts + one table per actor (customers, staff, technicians, managers, admins), sharing the id (TPT).
/// </summary>
internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts", t =>
        {
            t.HasEnumCheck<UserStatus>("status");
            t.HasCheckConstraint("ck_accounts_failed_login_count", "failed_login_count >= 0");
        });
        builder.UseTptMappingStrategy();

        // The snake_case convention skips TPT keys, so these tables keep EF's default "PK_accounts", "PK_customers"...
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();

        builder.Property(x => x.Username).HasMaxLength(50);
        builder.HasIndex(x => x.Username).IsUnique();
        builder.Property(x => x.PasswordHash).HasMaxLength(256);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.HasLookup(x => x.Role, "role_id");
        builder.HasIndex(x => x.Role);

        builder.Property(x => x.FullName).HasMaxLength(100);
        builder.Property(x => x.Email).HasMaxLength(254);
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.Status).HasEnumText();
    }
}

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder) => builder.ToTable("staff");
}

internal sealed class TechnicianConfiguration : IEntityTypeConfiguration<Technician>
{
    public void Configure(EntityTypeBuilder<Technician> builder)
    {
        builder.ToTable("technicians", t =>
            t.HasCheckConstraint("ck_technicians_hourly_rate", "hourly_rate IS NULL OR hourly_rate >= 0"));
        builder.Property(x => x.HourlyRate).HasPrecision(12, 2);
    }
}

internal sealed class ManagerConfiguration : IEntityTypeConfiguration<Manager>
{
    public void Configure(EntityTypeBuilder<Manager> builder) => builder.ToTable("managers");
}

internal sealed class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder) => builder.ToTable("admins");
}

internal sealed class SpecialtyConfiguration : IEntityTypeConfiguration<Specialty>
{
    public void Configure(EntityTypeBuilder<Specialty> builder)
    {
        builder.ToTable("specialties");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Name).HasMaxLength(60);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

internal sealed class TechnicianSpecialtyConfiguration : IEntityTypeConfiguration<TechnicianSpecialty>
{
    public void Configure(EntityTypeBuilder<TechnicianSpecialty> builder)
    {
        builder.ToTable("technician_specialties");
        builder.HasKey(x => new { x.TechnicianId, x.SpecialtyId });
        builder.HasOne(x => x.Technician).WithMany(x => x.Specialties).HasForeignKey(x => x.TechnicianId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Specialty).WithMany().HasForeignKey(x => x.SpecialtyId).OnDelete(DeleteBehavior.Restrict);
    }
}
