namespace SlaRequestProcessorCaseStudy.Tests;

// Verifies folder, file presence, and CSV header schema validation.
public class InputPathValidatorTests
{
    [Fact]
    // Missing folder should fail validation.
    public void ValidateFolderAndFiles_NonExistingFolder_ReturnsError()
    {
        // Arrange
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

		// Act
		string? result = InputPathValidator.ValidateFolderAndFiles(path);

		// Assert
		Assert.NotNull(result);
		Assert.Contains("invalid", result!, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	// Missing required CSV files should fail validation.
	public void ValidateFolderAndFiles_MissingRequiredFiles_ReturnsError()
	{
		// Arrange
		string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(tempDir);

		try
		{
			// Act
			string? result = InputPathValidator.ValidateFolderAndFiles(tempDir);

			// Assert
			Assert.NotNull(result);
			Assert.Contains("missing", result!, StringComparison.OrdinalIgnoreCase);
		}
		finally
		{
			Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	// All required files with valid headers should pass validation.
	public void ValidateFolderAndFiles_AllRequiredFilesPresent_ReturnsNull()
	{
		// Arrange
		string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(tempDir);

		string customersPath = Path.Combine(tempDir, "customers.csv");
		string tariffsPath = Path.Combine(tempDir, "tariffs.csv");
		string requestsPath = Path.Combine(tempDir, "requests.csv");

		File.WriteAllText(customersPath, "CustomerId;Name;HasUnpaidInvoice;SLA;MeterType\n");
		File.WriteAllText(tariffsPath,   "TariffId;Name;RequiresSmartMeter;BaseMonthlyGross\n");
		File.WriteAllText(requestsPath,  "RequestId;CustomerId;TargetTariffId;RequestedAtISO8601\n");
		try
		{
			// Act
			string? result = InputPathValidator.ValidateFolderAndFiles(tempDir);

			// Assert
			Assert.Null(result);
		}
		finally
		{
			Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	// Headers are matched case-insensitively (tricky: required for robustness).
	public void ValidateCsvSchema_CaseInsensitiveHeaders_ReturnsNull()
	{
		// Arrange
		string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(tempDir);

		File.WriteAllText(Path.Combine(tempDir, "customers.csv"), "customerId;name;HasUnpaidInvoice;SLA;MeterType");
		File.WriteAllText(Path.Combine(tempDir, "requests.csv"), "RequestId;CustomerId;targetTariffId;RequestedAtISO8601");
		File.WriteAllText(Path.Combine(tempDir, "tariffs.csv"), "TariffId;Name;RequiresSmartmeter;BaseMonthlyGross");

		try
		{
			// Act
			string? result = InputPathValidator.ValidateCsvSchema(tempDir);

			// Assert
			Assert.Null(result);
		}
		finally
		{
			Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	// Extra columns are allowed; only required headers are validated.
	public void ValidateCsvSchema_AllowsExtraColumns_ReturnsNull()
	{
		// Arrange
		string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(tempDir);

		File.WriteAllText(Path.Combine(tempDir, "customers.csv"), "CustomerId;Name;HasUnpaidInvoice;SLA;MeterType;ExtraColumn");
		File.WriteAllText(Path.Combine(tempDir, "requests.csv"), "RequestId;ExtraColumn;CustomerId;TargetTariffId;RequestedAtISO8601");
		File.WriteAllText(Path.Combine(tempDir, "tariffs.csv"), "ExtraColumn;TariffId;Name;RequiresSmartMeter;BaseMonthlyGross");

		try
		{
			// Act
			string? result = InputPathValidator.ValidateCsvSchema(tempDir);

			// Assert
			Assert.Null(result);
		}
		finally
		{
			Directory.Delete(tempDir, true);
		}
	}
	
	[Fact]
	// Missing required headers should fail validation and name the missing ones.
	public void ValidateCsvSchema_MissingHeaders_ReturnsError()
	{
		// Arrange
		string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(tempDir);

		File.WriteAllText(Path.Combine(tempDir, "customers.csv"), "CustomerId;Name;HasUnpaidInvoice;MeterType");
		File.WriteAllText(Path.Combine(tempDir, "requests.csv"), "RequestId;CustomerId;TargetTariffId;RequestedAtISO8601");
		File.WriteAllText(Path.Combine(tempDir, "tariffs.csv"), "TariffId;Name;RequiresSmartMeter;BaseMonthlyGross");

		try
		{
			// Act
			string? result = InputPathValidator.ValidateCsvSchema(tempDir);

			// Assert
			Assert.NotNull(result);
			Assert.Contains("SLA", result!, StringComparison.OrdinalIgnoreCase);
			Assert.Contains("customers.csv", result!, StringComparison.OrdinalIgnoreCase);
		}
		finally
		{
			Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	// Empty CSV file (no header) should fail validation.
	public void ValidateCsvSchema_EmptyFile_ReturnsError()
	{
		// Arrange
		string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(tempDir);

		File.WriteAllText(Path.Combine(tempDir, "customers.csv"), "");
		File.WriteAllText(Path.Combine(tempDir, "requests.csv"), "RequestId;CustomerId;TargetTariffId;RequestedAtISO8601");
		File.WriteAllText(Path.Combine(tempDir, "tariffs.csv"), "TariffId;Name;RequiresSmartMeter;BaseMonthlyGross");

		try
		{
			// Act
			string? result = InputPathValidator.ValidateCsvSchema(tempDir);

			// Assert
			Assert.NotNull(result);
			Assert.Contains("empty", result!, StringComparison.OrdinalIgnoreCase);
			Assert.Contains("customers.csv", result!, StringComparison.OrdinalIgnoreCase);
		}
		finally
		{
			Directory.Delete(tempDir, true);
		}
	}
}
