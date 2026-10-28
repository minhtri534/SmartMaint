using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartMaint.Api.Data.Conventions;
using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Data.Configurations.Cases;

internal sealed class PriorityLevelConfiguration : IEntityTypeConfiguration<PriorityLevel>
{
    public void Configure(EntityTypeBuilder<PriorityLevel> builder)
    {
        builder.ToTable("priority_levels", t =>
        {
            t.HasEnumCheck<CasePriority>("code");
            t.HasCheckConstraint("ck_priority_levels_sla_hours", "sla_hours > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Code).HasEnumText();
        builder.Property(x => x.Name).HasMaxLength(50);
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

internal sealed class CaseCategoryConfiguration : IEntityTypeConfiguration<CaseCategory>
{
    public void Configure(EntityTypeBuilder<CaseCategory> builder)
    {
        builder.ToTable("case_categories", t => t.HasEnumCheck<CostBearer>("default_cost_bearer"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.GroupName).HasMaxLength(60);
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(300);
        builder.Property(x => x.DefaultCostBearer).HasEnumText();
        builder.HasOne(x => x.DefaultPriority).WithMany().HasForeignKey(x => x.DefaultPriorityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.GroupName, x.Name }).IsUnique();
    }
}

internal sealed class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> builder)
    {
        builder.ToTable("cases", t =>
        {
            t.HasEnumCheck<CaseStatus>("status");
            t.HasEnumCheck<CostBearer>("cost_bearer");
            t.HasEnumCheck<HoldReason>("hold_reason");
            t.HasCheckConstraint("ck_cases_rating_stars", "rating_stars IS NULL OR rating_stars BETWEEN 1 AND 5");
            t.HasCheckConstraint("ck_cases_actual_hours", "actual_hours IS NULL OR actual_hours > 0");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();

        builder.Property(x => x.Title).HasMaxLength(120);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Status).HasEnumText();
        builder.Property(x => x.CostBearer).HasEnumText();
        builder.Property(x => x.HoldReason).HasEnumText();
        builder.Property(x => x.ActualHours).HasPrecision(5, 2);
        builder.Property(x => x.RootCause).HasMaxLength(1000);
        builder.Property(x => x.PartsUsed).HasMaxLength(500);
        builder.Property(x => x.RatingComment).HasMaxLength(500);

        builder.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.RequestingCustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Priority).WithMany().HasForeignKey(x => x.PriorityLevelId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Staff).WithMany().HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Technician).WithMany().HasForeignKey(x => x.AssignedTechnicianId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.RequestingCustomerId);
        builder.HasIndex(x => x.AssignedTechnicianId);
        builder.HasIndex(x => x.RoomId);
        builder.HasIndex(x => x.SubmittedAt);
    }
}

internal sealed class CaseImageConfiguration : IEntityTypeConfiguration<CaseImage>
{
    public void Configure(EntityTypeBuilder<CaseImage> builder)
    {
        builder.ToTable("case_images", t => t.HasEnumCheck<CaseImageKind>("kind"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Kind).HasEnumText();
        builder.HasOne(x => x.Case).WithMany(x => x.Images).HasForeignKey(x => x.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.CaseId);
    }
}

internal sealed class CaseStatusChangeConfiguration : IEntityTypeConfiguration<CaseStatusChange>
{
    public void Configure(EntityTypeBuilder<CaseStatusChange> builder)
    {
        builder.ToTable("case_status_history", t =>
        {
            t.HasEnumCheck<CaseStatus>("from_status");
            t.HasEnumCheck<CaseStatus>("to_status");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.FromStatus).HasEnumText();
        builder.Property(x => x.ToStatus).HasEnumText();
        builder.Property(x => x.Reason).HasMaxLength(600);
        builder.HasOne(x => x.Case).WithMany(x => x.History).HasForeignKey(x => x.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ChangedBy).WithMany().HasForeignKey(x => x.ChangedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.CaseId);
    }
}

internal sealed class CaseAssignmentConfiguration : IEntityTypeConfiguration<CaseAssignment>
{
    public void Configure(EntityTypeBuilder<CaseAssignment> builder)
    {
        builder.ToTable("case_assignments", t => t.HasEnumCheck<AssignmentAnswer>("response"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Response).HasEnumText();
        builder.Property(x => x.DeclineReason).HasMaxLength(600);
        builder.HasOne(x => x.Case).WithMany(x => x.Assignments).HasForeignKey(x => x.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Technician).WithMany().HasForeignKey(x => x.TechnicianId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AssignedBy).WithMany().HasForeignKey(x => x.AssignedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.CaseId);
        builder.HasIndex(x => x.TechnicianId);
    }
}
