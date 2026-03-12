using System.Globalization;

// Loads customers and tariffs from CSV into memory dictionaries.
public class CsvLookupLoader
{
    // Loads customers by ID; skips rows with missing/blank ID.
    public Dictionary<string, Customer> LoadCustomers(string path)
    {
        var customers = new Dictionary<string, Customer>(StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(path);

        string? header = reader.ReadLine();
        if (header == null)
            return customers;

        var idx = CsvHeaderMap.BuildHeaderIndex(header);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var cols = line.Split(';');

            string id = CsvHeaderMap.Get(cols, idx, "CustomerId");
            if (string.IsNullOrWhiteSpace(id))
                continue;

            // Parse boolean for unpaid invoice (defaults to false if unparseable).
            string name = CsvHeaderMap.Get(cols, idx, "Name");
            bool hasUnpaid = bool.TryParse(CsvHeaderMap.Get(cols, idx, "HasUnpaidInvoice"), out var unpaid) && unpaid;
            string sla = CsvHeaderMap.Get(cols, idx, "SLA");
            string meterType = CsvHeaderMap.Get(cols, idx, "MeterType");

            customers[id] = new Customer(id, name, hasUnpaid, sla, meterType);
        }

        return customers;
    }

    // Loads tariffs by ID; skips rows with missing ID or unparseable monthly price.
    public Dictionary<string, Tariff> LoadTariffs(string path)
    {
        var tariffs = new Dictionary<string, Tariff>(StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(path);

        string? header = reader.ReadLine();
        if (header == null)
            return tariffs;

        var idx = CsvHeaderMap.BuildHeaderIndex(header);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var cols = line.Split(';');

            string id = CsvHeaderMap.Get(cols, idx, "TariffId");
            if (string.IsNullOrWhiteSpace(id))
                continue;

            // Parse boolean for smart meter requirement (defaults to false if unparseable).
            string name = CsvHeaderMap.Get(cols, idx, "Name");
            bool requiresSmart = bool.TryParse(CsvHeaderMap.Get(cols, idx, "RequiresSmartMeter"), out var r) && r;

            // Tricky: use InvariantCulture for decimal parsing; skip row if price unparseable.
            if (!decimal.TryParse(
                    CsvHeaderMap.Get(cols, idx, "BaseMonthlyGross"),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var gross))
                continue;

            // Row is valid; add to lookup dictionary.
            tariffs[id] = new Tariff(id, name, requiresSmart, gross);
        }

        return tariffs;
    }

}
