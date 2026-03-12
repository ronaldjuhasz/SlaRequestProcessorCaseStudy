namespace SlaRequestProcessorCaseStudy.Tests;
// Verifies CSV loading handles flexible column order and extra columns.
public class CsvLookupLoaderTests
{
    [Fact]
    // Customers load correctly even with shuffled columns and extra data.
    public void LoadCustomers_AllowsExtraColumnsAndShuffledOrder()
    {
        // Arrange
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(root);

        string customersPath = Path.Combine(root, "customers.csv");

        File.WriteAllLines(customersPath, new[]
        {
            "Name;MeterType;CustomerId;ExtraCol;HasUnpaidInvoice;SLA",
            "Alice;Smart;C001;whatever;false;Premium",
            "Bob;Classic;C002;zzz;true;Standard"
        });

        var loader = new CsvLookupLoader();

        // Act
        var customers = loader.LoadCustomers(customersPath);

        // Assert
        Assert.Equal(2, customers.Count);

        Assert.True(customers.ContainsKey("C001"));
        Assert.Equal("Alice", customers["C001"].Name);
        Assert.False(customers["C001"].HasUnpaidInvoice);
        Assert.Equal("Premium", customers["C001"].SLA);
        Assert.Equal("Smart", customers["C001"].MeterType);

        Assert.True(customers.ContainsKey("C002"));
        Assert.Equal("Bob", customers["C002"].Name);
        Assert.True(customers["C002"].HasUnpaidInvoice);
        Assert.Equal("Standard", customers["C002"].SLA);
        Assert.Equal("Classic", customers["C002"].MeterType);
    }

    [Fact]
    // Tariffs load correctly even with shuffled columns, extra data, and decimal parsing.
    public void LoadTariffs_AllowsExtraColumnsAndShuffledOrder()
    {
        // Arrange
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(root);

        string tariffsPath = Path.Combine(root, "tariffs.csv");

        File.WriteAllLines(tariffsPath, new[]
        {
            "Name;Extra;BaseMonthlyGross;TariffId;RequiresSmartMeter",
            "Basic Plan;foo;19.99;T001;false",
            "Smart Plan;bar;29.50;T002;true"
        });

        var loader = new CsvLookupLoader();

        // Act
        var tariffs = loader.LoadTariffs(tariffsPath);

        // Assert
        Assert.Equal(2, tariffs.Count);

        Assert.True(tariffs.ContainsKey("T001"));
        Assert.Equal("Basic Plan", tariffs["T001"].Name);
        Assert.False(tariffs["T001"].RequiresSmartMeter);
        Assert.Equal(19.99m, tariffs["T001"].BaseMonthlyGross);

        Assert.True(tariffs.ContainsKey("T002"));
        Assert.Equal("Smart Plan", tariffs["T002"].Name);
        Assert.True(tariffs["T002"].RequiresSmartMeter);
        Assert.Equal(29.50m, tariffs["T002"].BaseMonthlyGross);
    }
}
