using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartMaint.Api.Data.Conventions;
using SmartMaint.Api.Entities.Users;

namespace SmartMaint.Api.Data.Configurations.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", t =>
        {
            t.HasEnumCheck<UserRole>("role");
            t.HasEnumCheck<UserStatus>("status");
            t.HasCheckConstraint("ck_users_failed_login_count", "failed_login_count >= 0");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();

        builder.Property(x => x.Email).HasMaxLength(254);
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.FullName).HasMaxLength(100);
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.Role).HasEnumText();
        builder.Property(x => x.Status).HasEnumText();
        builder.Property(x => x.PasswordHash).HasMaxLength(256);

        builder.HasIndex(x => x.Role);
    }
}
