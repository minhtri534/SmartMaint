using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartMaint.Api.Data.Conventions;
using SmartMaint.Api.Entities.Core;

namespace SmartMaint.Api.Data.Configurations.Core;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.HasLookup(x => x.TargetType, "target_type_id");
        builder.HasLookup(x => x.Operation, "operation_id");
        builder.Property(x => x.OldValue).HasColumnType("text");
        builder.Property(x => x.NewValue).HasColumnType("text");
        builder.HasOne(x => x.Admin).WithMany().HasForeignKey(x => x.AdminId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.OccurredAt);
        builder.HasIndex(x => x.AdminId);
    }
}

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Details).HasMaxLength(500);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.UserId, x.SentAt });
    }
}

internal sealed class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation>
{
    public void Configure(EntityTypeBuilder<AiConversation> builder)
    {
        builder.ToTable("ai_conversations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.ContextType).HasMaxLength(30);
        // ERD draws Account - AIConversation as 1-1; 1-M since 16/11 (ERD review #5).
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class AiMessageConfiguration : IEntityTypeConfiguration<AiMessage>
{
    public void Configure(EntityTypeBuilder<AiMessage> builder)
    {
        builder.ToTable("ai_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        // 2000 = the longest case description (CreateCaseRequestValidator); was 500.
        builder.Property(x => x.Message).HasMaxLength(2000);
        builder.Property(x => x.Reply).HasColumnType("text");
        // ERD draws AIConversation - AIMessage as 1-1; 1-M since 16/11 (ERD review #5).
        builder.HasOne(x => x.Conversation).WithMany(x => x.Messages)
            .HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
    }
}
