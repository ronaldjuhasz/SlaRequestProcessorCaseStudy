namespace SlaRequestProcessorCaseStudy.Tests;

// Integration tests for the main request processing pipeline.
public class RequestProcessorTests
{
    [Fact]
    // Approves and creates meter upgrade follow-up when tariff requires smart meter.
    public void Run_ApprovesAndCreatesFollowUp_WhenSmartMeterUpgradeIsRequired()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            Directory.CreateDirectory(root);
            var files = WriteInputFiles(
                root,
                customersRows:
                [
                    "C1;Alice;false;Premium;Classic"
                ],
                tariffsRows:
                [
                    "T1;Smart Plan;true;29.50"
                ],
                requestsRows:
                [
                    "R1;C1;T1;2025-06-15T10:00:00+02:00"
                ]);

            string outputFolder = Path.Combine(root, "out");
            string processedIdsPath = Path.Combine(root, "processed", "processed_ids.csv");

            var processor = new RequestProcessor(files, processedIdsPath, new DecisionWriter(outputFolder));

            processor.Run();

            string decisionsPath = Path.Combine(outputFolder, "decisions.csv");
            string followupsPath = Path.Combine(outputFolder, "followups.csv");

            string[] decisionLines = File.ReadAllLines(decisionsPath);
            string[] followUpLines = File.ReadAllLines(followupsPath);

            Assert.Equal(2, decisionLines.Length);
            Assert.Equal("R1;Approve;;2025-06-16T22:00:00+02:00", decisionLines[1]);

            Assert.Equal(2, followUpLines.Length);
            Assert.Equal("R1;Schedule meter upgrade;2025-06-16T10:00:00+02:00", followUpLines[1]);

            string[] processedIds = File.ReadAllLines(processedIdsPath);
            Assert.Single(processedIds);
            Assert.Equal("R1", processedIds[0]);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    // Rejects request with unknown customer and marks as processed.
    public void Run_RejectsUnknownCustomer_WritesDecisionAndMarksAsProcessed()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            Directory.CreateDirectory(root);
            var files = WriteInputFiles(
                root,
                customersRows:
                [
                    "C1;Alice;false;Standard;Smart"
                ],
                tariffsRows:
                [
                    "T1;Basic Plan;false;19.99"
                ],
                requestsRows:
                [
                    "R2;UNKNOWN;T1;2025-06-15T10:00:00+02:00"
                ]);

            string outputFolder = Path.Combine(root, "out");
            string processedIdsPath = Path.Combine(root, "processed", "processed_ids.csv");

            var processor = new RequestProcessor(files, processedIdsPath, new DecisionWriter(outputFolder));

            processor.Run();

            string decisionsPath = Path.Combine(outputFolder, "decisions.csv");
            string[] decisionLines = File.ReadAllLines(decisionsPath);

            Assert.Equal(2, decisionLines.Length);
            Assert.Equal("R2;Reject;Unknown customer;N/A", decisionLines[1]);

            string[] processedIds = File.ReadAllLines(processedIdsPath);
            Assert.Single(processedIds);
            Assert.Equal("R2", processedIds[0]);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    // Skips already-processed request, does not add rows to output files.
    public void Run_SkipsAlreadyProcessedRequest_LeavesOnlyHeadersInOutputFiles()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            Directory.CreateDirectory(root);
            var files = WriteInputFiles(
                root,
                customersRows:
                [
                    "C1;Alice;false;Standard;Smart"
                ],
                tariffsRows:
                [
                    "T1;Basic Plan;false;19.99"
                ],
                requestsRows:
                [
                    "R3;C1;T1;2025-06-15T10:00:00+02:00"
                ]);

            string outputFolder = Path.Combine(root, "out");
            string processedIdsPath = Path.Combine(root, "processed", "processed_ids.csv");

            Directory.CreateDirectory(Path.GetDirectoryName(processedIdsPath)!);
            File.WriteAllText(processedIdsPath, "R3" + Environment.NewLine);

            var processor = new RequestProcessor(files, processedIdsPath, new DecisionWriter(outputFolder));

            processor.Run();

            string decisionsPath = Path.Combine(outputFolder, "decisions.csv");
            string followupsPath = Path.Combine(outputFolder, "followups.csv");

            string[] decisionLines = File.ReadAllLines(decisionsPath);
            string[] followUpLines = File.ReadAllLines(followupsPath);

            Assert.Single(decisionLines);
            Assert.Single(followUpLines);

            string[] processedIds = File.ReadAllLines(processedIdsPath);
            Assert.Single(processedIds);
            Assert.Equal("R3", processedIds[0]);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    // Helper: creates temp CSV files with given rows (headers auto-added). Tricky: uses spread operator for concise row list building.
    private static InputFiles WriteInputFiles(
        string root,
        string[] customersRows,
        string[] tariffsRows,
        string[] requestsRows)
    {
        string customersPath = Path.Combine(root, "customers.csv");
        string tariffsPath = Path.Combine(root, "tariffs.csv");
        string requestsPath = Path.Combine(root, "requests.csv");

        File.WriteAllLines(customersPath,
        [
            "CustomerId;Name;HasUnpaidInvoice;SLA;MeterType",
            ..customersRows
        ]);

        File.WriteAllLines(tariffsPath,
        [
            "TariffId;Name;RequiresSmartMeter;BaseMonthlyGross",
            ..tariffsRows
        ]);

        File.WriteAllLines(requestsPath,
        [
            "RequestId;CustomerId;TargetTariffId;RequestedAtISO8601",
            ..requestsRows
        ]);

        return new InputFiles(customersPath, tariffsPath, requestsPath);
    }
}
