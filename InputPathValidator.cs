// Validates input folder, required files, and CSV header schemas.
public class InputPathValidator
{
    private static readonly Dictionary<string, string[]> RequiredSchemaByFile = new()
    {
        { "customers.csv", new[] { "CustomerId", "Name", "HasUnpaidInvoice", "SLA", "MeterType" } },
        { "requests.csv",  new[] { "RequestId", "CustomerId", "TargetTariffId", "RequestedAtISO8601" } },
        { "tariffs.csv",   new[] { "TariffId", "Name", "RequiresSmartMeter", "BaseMonthlyGross" } }
    };

    // Validates folder exists and all required files with correct headers are present.
    public static string? ValidateFolderAndFiles(string path)
    {
        if (!Directory.Exists(path))
            return "The provided input path is invalid. Please check the path and try again.";

        string? fileCheckError = AreFilesPresent(path);
        if (fileCheckError != null)
            return fileCheckError;

        return ValidateCsvSchema(path);
    }

    // Builds InputFiles object with full paths to the three CSV files.
    public static InputFiles BuildInputFiles(string path)
    {
        return new InputFiles(
            Path.Combine(path, "customers.csv"),
            Path.Combine(path, "tariffs.csv"),
            Path.Combine(path, "requests.csv")
        );
    }

    // Checks all three CSV files exist at the given path.
    private static string? AreFilesPresent(string path)
    {
		foreach (string file in RequiredSchemaByFile.Keys)
		{
            string fullPath = Path.Combine(path, file);
            if (!File.Exists(fullPath))
                return $"The provided input path is missing the required file: {file}. Please check the path and try again.";
        }
        return null;
    }

    // Reads first line of a file (typically the header).
    private static string? ReadFirstLine(string filePath)
    {
        using var reader = new StreamReader(filePath);
        return reader.ReadLine();
    }

    // Splits header by semicolon and trims whitespace.
    private static string[] ParseHeaderLine(string headerLine)
    {
        return [.. headerLine.Split(';').Select(h => h.Trim())];
    }

    // Validates each CSV file has all required headers (tricky: case-insensitive, allows extra columns).
    public static string? ValidateCsvSchema(string path)
    {
        foreach (var kvp in RequiredSchemaByFile)
        {
            string filePath = Path.Combine(path, kvp.Key);
            string[] expectedHeaders = kvp.Value;

            string? headerLine = ReadFirstLine(filePath);
            if (headerLine == null)
                return $"The file {kvp.Key} is empty. Please check the file and try again.";

            string[] actualHeaders = ParseHeaderLine(headerLine);
            // Tricky: case-insensitive header matching.
            var headerSet = new HashSet<string>(actualHeaders, StringComparer.OrdinalIgnoreCase);

            List<string> missingHeaders = [];
            foreach (string expectedHeader in expectedHeaders)
                if (!headerSet.Contains(expectedHeader))
                    missingHeaders.Add(expectedHeader);

            if (missingHeaders.Count > 0)
                return $"The file {kvp.Key} is missing the required headers: {string.Join(", ", missingHeaders)}. Please check the file and try again.";
        }

        return null;
    }
}
