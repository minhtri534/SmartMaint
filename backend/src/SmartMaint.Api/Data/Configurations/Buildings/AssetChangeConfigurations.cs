using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartMaint.Api.Data.Conventions;
using SmartMaint.Api.Entities.Buildings;

namespace SmartMaint.Api.Data.Configurations.Buildings;

// NOT IN ERD v5 (meeting note 18/11): change lists of floors, rooms and assets, approved by a manager.

internal sealed class AssetChangeListConfiguration : IEntityTypeConfiguration<AssetChangeList>
{
    public void Configure(EntityTypeBuilder<AssetChangeList> builder)
    {
        builder.ToTable("asset_change_lists", t => t.HasEnumCheck<AssetChangeListStatus>("status"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Title).HasMaxLength(120);
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.Status).HasEnumText();
        builder.Property(x => x.RejectReason).HasMaxLength(500);
        builder.HasOne(x => x.Building).WithMany().HasForeignKey(x => x.BuildingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Staff).WithMany().HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Reviewer).WithMany().HasForeignKey(x => x.ReviewerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.BuildingId, x.Status });
    }
}

internal sealed class AssetChangeItemConfiguration : IEntityTypeConfiguration<AssetChangeItem>
{
    public void Configure(EntityTypeBuilder<AssetChangeItem> builder)
    {
        builder.ToTable("asset_change_items", t => t.HasEnumCheck<AssetChangeKind>("kind"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Kind).HasEnumText();
        builder.Property(x => x.Key).HasMaxLength(20);
        builder.Property(x => x.ParentKey).HasMaxLength(20);
        builder.Property(x => x.Name).HasMaxLength(150);
        builder.Property(x => x.Address).HasMaxLength(200);
        builder.Property(x => x.TargetLabel).HasMaxLength(300);
        builder.HasOne(x => x.List).WithMany(x => x.Items).HasForeignKey(x => x.ListId).OnDelete(DeleteBehavior.Cascade);
        // A line keeps its words (TargetLabel) when the floor, room or asset it pointed at is deleted.
        builder.HasOne(x => x.Building).WithMany().HasForeignKey(x => x.BuildingId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Floor).WithMany().HasForeignKey(x => x.FloorId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Type).WithMany().HasForeignKey(x => x.TypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Manufacturer).WithMany().HasForeignKey(x => x.ManufacturerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ListId, x.Key }).IsUnique();
    }
}
