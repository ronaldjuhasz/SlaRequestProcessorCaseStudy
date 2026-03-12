namespace SlaRequestProcessorCaseStudy.Tests;

// Basic behavior tests for CSV output writer.
public class DecisionWriterTests
{
    [Fact]
    // Verifies header files are created on initialization.
    public void Initialize_CreatesCsvFilesWithHeaders()
    {
        // Isolated temp folder per test run.
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            var writer = new DecisionWriter(root);

            writer.Initialize();

            string decisionsPath = Path.Combine(root, "decisions.csv");
            string followupsPath = Path.Combine(root, "followups.csv");

            Assert.True(File.Exists(decisionsPath));
            Assert.True(File.Exists(followupsPath));

            string decisionsHeader = File.ReadLines(decisionsPath).First();
            string followupsHeader = File.ReadLines(followupsPath).First();

            Assert.Equal("RequestId;Decision;Reason;SlaDueISO8601", decisionsHeader);
            Assert.Equal("RequestId;Action;DeadlineISO8601", followupsHeader);
        }
        finally
        {
            // Cleanup keeps local temp storage tidy.
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    // Verifies rows are appended with expected formatting.
    public void AppendDecisionAndFollowUp_WritesExpectedRows()
    {
        // Isolated temp folder per test run.
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            var writer = new DecisionWriter(root);
            writer.Initialize();

            var slaDue = new DateTimeOffset(2025, 6, 16, 22, 0, 0, TimeSpan.FromHours(2));
            var deadline = new DateTimeOffset(2025, 6, 16, 10, 0, 0, TimeSpan.FromHours(2));

            writer.AppendDecision("R-1", "Approve", "", slaDue);
            writer.AppendDecision("R-2", "Reject", "Unknown customer", null);
            writer.AppendFollowUp("R-1", "Schedule meter upgrade", deadline);

            string decisionsPath = Path.Combine(root, "decisions.csv");
            string followupsPath = Path.Combine(root, "followups.csv");

            string[] decisionLines = File.ReadAllLines(decisionsPath);
            string[] followUpLines = File.ReadAllLines(followupsPath);

            Assert.Equal(3, decisionLines.Length);
            Assert.Equal("R-1;Approve;;2025-06-16T22:00:00+02:00", decisionLines[1]);
            Assert.Equal("R-2;Reject;Unknown customer;N/A", decisionLines[2]);

            Assert.Equal(2, followUpLines.Length);
            Assert.Equal("R-1;Schedule meter upgrade;2025-06-16T10:00:00+02:00", followUpLines[1]);
        }
        finally
        {
            // Cleanup keeps local temp storage tidy.
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }
}
