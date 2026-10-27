using System.Text;

namespace SmartMaint.Api.Data.Conventions;

/// <summary>C# enum member &lt;-&gt; snake_case text stored in the database (UserRole.Admin &lt;-&gt; "admin").</summary>
public static class EnumText
{
    public static string ToText<TEnum>(TEnum value) where TEnum : struct, Enum => ToSnakeCase(value.ToString());

    public static TEnum FromText<TEnum>(string text) where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>().Single(v => ToText(v) == text);

    public static IEnumerable<string> AllTexts<TEnum>() where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>().Select(ToText);

    private static string ToSnakeCase(string name)
    {
        var builder = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0)
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(name[i]));
        }

        return builder.ToString();
    }
}
