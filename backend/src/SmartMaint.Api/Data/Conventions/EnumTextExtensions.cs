using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartMaint.Api.Data.Conventions;

/// <summary>
/// Convention "enum stored as text + CHECK" (D-plan). Use both together for every enum column:
/// <code>
/// builder.ToTable("users", t => t.HasEnumCheck&lt;UserRole&gt;("role"));
/// builder.Property(x => x.Role).HasEnumText();
/// </code>
/// </summary>
public static class EnumTextExtensions
{
    private const int MaxLength = 32;

    public static PropertyBuilder<TEnum> HasEnumText<TEnum>(this PropertyBuilder<TEnum> property)
        where TEnum : struct, Enum =>
        property
            .HasConversion(v => EnumText.ToText(v), v => EnumText.FromText<TEnum>(v))
            .HasMaxLength(MaxLength);

    public static void HasEnumCheck<TEnum>(this TableBuilder table, string column)
        where TEnum : struct, Enum
    {
        var values = string.Join(", ", EnumText.AllTexts<TEnum>().Select(v => $"'{v}'"));
        table.HasCheckConstraint($"ck_{table.Name}_{column}", $"{column} IN ({values})");
    }
}
