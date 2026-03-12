// Helper: maps CSV header names to column indices and safely retrieves values.
public static class CsvHeaderMap
{
    // Builds case-insensitive header-to-column-index map; tricky: removes BOM if present.
    public static Dictionary<string, int> BuildHeaderIndex(string headerLine)
    {
        headerLine = headerLine.TrimStart('\uFEFF'); // BOM safety
        var headers = headerLine.Split(';').Select(h => h.Trim()).ToArray();

        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
            map[headers[i]] = i;

        return map;
    }

    // Safely retrieves column value by name; returns empty string if not found or out-of-bounds.
    public static string Get(string[] cols, Dictionary<string, int> idx, string name)
    {
        return idx.TryGetValue(name, out int i) && i < cols.Length
            ? cols[i].Trim()
            : "";
    }
}
