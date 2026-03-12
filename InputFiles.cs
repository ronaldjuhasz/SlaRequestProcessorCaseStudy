// Holds file paths for the three input CSV files.
public sealed class InputFiles
{
    public string CustomersPath { get; }
    public string TariffsPath { get; }
    public string RequestsPath { get; }

    // Simple immutable record of the three input file paths.
    public InputFiles(string customersPath, string tariffsPath, string requestsPath)
    {
        CustomersPath = customersPath;
        TariffsPath = tariffsPath;
        RequestsPath = requestsPath;
    }
}
