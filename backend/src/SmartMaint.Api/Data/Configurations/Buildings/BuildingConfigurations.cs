using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartMaint.Api.Entities.Buildings;

namespace SmartMaint.Api.Data.Configurations.Buildings;

internal sealed class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("buildings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.Address).HasMaxLength(200);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

internal sealed class FloorConfiguration : IEntityTypeConfiguration<Floor>
{
    public void Configure(EntityTypeBuilder<Floor> builder)
    {
        builder.ToTable("floors");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Name).HasMaxLength(50);
        builder.HasOne(x => x.Building).WithMany(x => x.Floors).HasForeignKey(x => x.BuildingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.BuildingId, x.Name }).IsUnique();
    }
}

internal sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("rooms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.HasOne(x => x.Floor).WithMany(x => x.Rooms).HasForeignKey(x => x.FloorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.FloorId, x.Name }).IsUnique();
    }
}

internal sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("assets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Name).HasMaxLength(150);
        builder.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.RoomId, x.Name }).IsUnique();
    }
}

internal sealed class StaffBuildingAssignmentConfiguration : IEntityTypeConfiguration<StaffBuildingAssignment>
{
    public void Configure(EntityTypeBuilder<StaffBuildingAssignment> builder)
    {
        builder.ToTable("staff_building_assignments");
        builder.HasKey(x => new { x.StaffId, x.BuildingId });
        builder.HasOne(x => x.Staff).WithMany().HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Building).WithMany().HasForeignKey(x => x.BuildingId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ManagerBuildingAssignmentConfiguration : IEntityTypeConfiguration<ManagerBuildingAssignment>
{
    public void Configure(EntityTypeBuilder<ManagerBuildingAssignment> builder)
    {
        builder.ToTable("manager_building_assignments");
        builder.HasKey(x => new { x.ManagerId, x.BuildingId });
        builder.HasOne(x => x.Manager).WithMany().HasForeignKey(x => x.ManagerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Building).WithMany().HasForeignKey(x => x.BuildingId).OnDelete(DeleteBehavior.Restrict);
    }
}
