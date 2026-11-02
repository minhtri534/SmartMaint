using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartMaint.Api.Data.Conventions;
using SmartMaint.Api.Entities.Maintenance;

namespace SmartMaint.Api.Data.Configurations.Maintenance;

internal sealed class CaseChecklistConfiguration : IEntityTypeConfiguration<CaseChecklist>
{
    public void Configure(EntityTypeBuilder<CaseChecklist> builder)
    {
        builder.ToTable("case_checklists");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Name).HasMaxLength(120);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

internal sealed class ChecklistTaskConfiguration : IEntityTypeConfiguration<ChecklistTask>
{
    public void Configure(EntityTypeBuilder<ChecklistTask> builder)
    {
        builder.ToTable("checklist_tasks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasOne(x => x.Checklist).WithMany(x => x.Tasks).HasForeignKey(x => x.ChecklistId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class MaintenanceTriggerConfiguration : IEntityTypeConfiguration<MaintenanceTrigger>
{
    public void Configure(EntityTypeBuilder<MaintenanceTrigger> builder)
    {
        builder.ToTable("maintenance_triggers", t =>
        {
            t.HasCheckConstraint("ck_maintenance_triggers_frequency", "frequency > 0");
            t.HasCheckConstraint("ck_maintenance_triggers_lead_days", "lead_days >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.HasLookup(x => x.FrequencyType, "frequency_type_id");
        builder.HasLookup(x => x.Status, "status_id");
        builder.Property(x => x.RejectReason).HasMaxLength(500);
        builder.HasOne(x => x.Staff).WithMany().HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ApprovingManager).WithMany().HasForeignKey(x => x.ApprovingManagerId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class MaintenanceWorkOrderDetailConfiguration : IEntityTypeConfiguration<MaintenanceWorkOrderDetail>
{
    public void Configure(EntityTypeBuilder<MaintenanceWorkOrderDetail> builder)
    {
        builder.ToTable("maintenance_work_order_details", t =>
            t.HasCheckConstraint("ck_maintenance_work_order_details_estimated_hours", "estimated_hours IS NULL OR estimated_hours > 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.EstimatedHours).HasPrecision(6, 2);
        builder.HasOne(x => x.Trigger).WithMany(x => x.Details).HasForeignKey(x => x.TriggerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Checklist).WithMany().HasForeignKey(x => x.ChecklistId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PriorityLevel).WithMany().HasForeignKey(x => x.PriorityLevelId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class MaintenanceWorkOrderAssetConfiguration : IEntityTypeConfiguration<MaintenanceWorkOrderAsset>
{
    public void Configure(EntityTypeBuilder<MaintenanceWorkOrderAsset> builder)
    {
        builder.ToTable("maintenance_work_order_assets");
        // ERD review #15: the pair (detail, asset) is the key.
        builder.HasKey(x => new { x.WorkOrderDetailId, x.AssetId });
        builder.HasOne(x => x.WorkOrderDetail).WithMany(x => x.Assets).HasForeignKey(x => x.WorkOrderDetailId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Technician).WithMany().HasForeignKey(x => x.TechnicianId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CaseTaskConfiguration : IEntityTypeConfiguration<CaseTask>
{
    public void Configure(EntityTypeBuilder<CaseTask> builder)
    {
        builder.ToTable("case_tasks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasLookup(x => x.Status, "status_id");
        builder.HasOne(x => x.Case).WithMany(x => x.Tasks).HasForeignKey(x => x.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.RepairCase).WithMany().HasForeignKey(x => x.RepairCaseId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.CaseId);
    }
}

internal sealed class TaskNoteConfiguration : IEntityTypeConfiguration<TaskNote>
{
    public void Configure(EntityTypeBuilder<TaskNote> builder)
    {
        builder.ToTable("task_notes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.HasOne(x => x.Task).WithMany(x => x.Notes).HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class TaskImageConfiguration : IEntityTypeConfiguration<TaskImage>
{
    public void Configure(EntityTypeBuilder<TaskImage> builder)
    {
        builder.ToTable("task_images");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Image).HasMaxLength(300);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.HasOne(x => x.Task).WithMany(x => x.Images).HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
    }
}
