// Persists and tracks processed request IDs to enable idempotent reruns.
public class ProcessedRequestStore
{
    private readonly string filePath;
    private readonly HashSet<string> processedIds = [];

    // Constructor sets persistence file path (defaults to ./processedRequestIDs/processed_ids.csv).
    public ProcessedRequestStore(string? filePath = null)
    {
        this.filePath = filePath ?? "./processedRequestIDs/processed_ids.csv";
    }

    // Loads existing IDs from file into memory; creates file/folder if missing; returns count loaded.
    public int Load()
    {
        string? dir = Path.GetDirectoryName(filePath);

        // Ensure folder exists.
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if (!File.Exists(filePath))
        {
            // Create empty file if it doesn't exist.
            File.Create(filePath).Dispose();
            return 0;
        }

        foreach (var line in File.ReadLines(filePath))
        {
            if (!string.IsNullOrWhiteSpace(line))
                // Trim whitespace and skip empty lines.
                processedIds.Add(line.Trim());
        }

        return processedIds.Count;
    }

    // Checks if ID has been processed.
    public bool IsProcessed(string id) => processedIds.Contains(id);

    // Marks a new ID as processed and appends to file (tricky: idempotent — only adds once).
    public void MarkProcessed(string id)
    {
        if (!processedIds.Add(id))
            return;

        File.AppendAllLines(filePath, new[] { id });
    }
}
