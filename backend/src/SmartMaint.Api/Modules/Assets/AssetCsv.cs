using System.Text;

namespace SmartMaint.Api.Modules.Assets;

/// <summary>
/// AST-05 file format. One header row, then one asset per row, comma separated, quotes allowed ("a, b").
/// Columns (any order, header names as below): building, floor, room, name, type_code, manufacturer,
/// has_copyright (yes/no), warranty_end_date (yyyy-MM-dd). manufacturer, has_copyright and warranty_end_date may be empty.
/// </summary>
internal static class AssetCsv
{
    public static readonly string[] Columns =
        ["building", "floor", "room", "name", "type_code", "manufacturer", "has_copyright", "warranty_end_date"];

    public static readonly string[] RequiredColumns = ["building", "floor", "room", "name", "type_code"];

    public const int MaxRows = 1000;

    public sealed record Row(int Line, IReadOnlyDictionary<string, string> Values)
    {
        public string this[string column] => Values.TryGetValue(column, out var v) ? v : string.Empty;
    }

    /// <summary>Rows keyed by header name (lower case). Throws <see cref="FormatException"/> on a bad header.</summary>
    public static List<Row> Parse(string csv)
    {
        var lines = SplitRecords(csv).ToList();
        if (lines.Count == 0)
        {
            throw new FormatException("The file is empty.");
        }

        var header = lines[0].Fields.Select(h => h.Trim().ToLowerInvariant()).ToList();
        var missing = RequiredColumns.Where(c => !header.Contains(c)).ToList();
        if (missing.Count > 0)
        {
            throw new FormatException($"Missing column(s): {string.Join(", ", missing)}.");
        }

        return lines.Skip(1)
            .Where(l => l.Fields.Any(f => !string.IsNullOrWhiteSpace(f)))
            .Select(l => new Row(
                l.Line,
                header.Select((h, i) => (h, v: i < l.Fields.Count ? l.Fields[i].Trim() : string.Empty))
                    .GroupBy(x => x.h).ToDictionary(g => g.Key, g => g.First().v)))
            .ToList();
    }

    /// <summary>A small RFC 4180 reader: quoted fields may hold commas, double quotes ("") and line breaks.</summary>
    private static IEnumerable<(int Line, List<string> Fields)> SplitRecords(string csv)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var line = 1;
        var recordLine = 1;

        for (var i = 0; i < csv.Length; i++)
        {
            var c = csv[i];
            if (inQuotes)
            {
                if (c == '"' && i + 1 < csv.Length && csv[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    inQuotes = false;
                }
                else
                {
                    if (c == '\n') line++;
                    field.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == ',')
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else if (c == '\n' || c == '\r')
            {
                if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n') i++;
                fields.Add(field.ToString());
                field.Clear();
                yield return (recordLine, fields);
                fields = [];
                line++;
                recordLine = line;
            }
            else
            {
                field.Append(c);
            }
        }

        if (field.Length > 0 || fields.Count > 0)
        {
            fields.Add(field.ToString());
            yield return (recordLine, fields);
        }
    }
}
