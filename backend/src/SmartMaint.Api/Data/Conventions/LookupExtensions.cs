using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartMaint.Api.Entities;

namespace SmartMaint.Api.Data.Conventions;

/// <summary>
/// Convention "ERD lookup table" (Roles, CaseStatuses...). Use both together:
/// <code>
/// modelBuilder.HasLookupTable&lt;CaseStatus&gt;("case_statuses");      // in AppDbContext: table + one row per enum member
/// builder.HasLookup(x =&gt; x.Status, "status_id");                    // in the entity configuration: FK column
/// </code>
/// </summary>
public static class LookupExtensions
{
    public static void HasLookupTable<TEnum>(this ModelBuilder modelBuilder, string tableName)
        where TEnum : struct, Enum
    {
        var table = modelBuilder.Entity<Lookup<TEnum>>();
        table.ToTable(tableName);
        table.HasKey(x => x.Id);
        table.Property(x => x.Id).HasConversion<long>().ValueGeneratedNever();
        table.Property(x => x.Name).HasMaxLength(50);
        table.HasIndex(x => x.Name).IsUnique();
        table.HasData(Enum.GetValues<TEnum>().Select(v => new Lookup<TEnum> { Id = v, Name = EnumText.ToText(v) }));
    }

    /// <summary>An enum property stored as the id of its lookup row, with a real FK.</summary>
    public static void HasLookup<TEntity, TEnum>(
        this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, TEnum>> property, string column)
        where TEntity : class
        where TEnum : struct, Enum
    {
        builder.Property(property).HasConversion<long>().HasColumnName(column);
        builder.HasOne<Lookup<TEnum>>().WithMany().HasForeignKey(PropertyName(property)).OnDelete(DeleteBehavior.Restrict);
    }

    private static string PropertyName(LambdaExpression property) => ((MemberExpression)property.Body).Member.Name;
}
