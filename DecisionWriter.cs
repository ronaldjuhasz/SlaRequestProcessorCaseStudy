using System;

// Writes request decisions and follow-up tasks to CSV files.
public class DecisionWriter
{
    private readonly string outputFolder;
    private readonly string decisionsFilePath;
    private readonly string followUpsFilePath;

    // Constructor sets output folder and paths (defaults to ./outputFiles).
    public DecisionWriter(string outputFolder = "./outputFiles")
    {
        this.outputFolder = outputFolder;
        decisionsFilePath = Path.Combine(outputFolder, "decisions.csv");
        followUpsFilePath = Path.Combine(outputFolder, "followups.csv");
    }

    // Creates output folder and initializes CSV files with headers (overwrites existing).
    public void Initialize()
    {
        Directory.CreateDirectory(outputFolder);

        // Tricky: Initialize overwrites existing files; combined with ProcessedRequestStore idempotency allows reruns.
        File.WriteAllText(decisionsFilePath, "RequestId;Decision;Reason;SlaDueISO8601" + Environment.NewLine);

        File.WriteAllText(followUpsFilePath, "RequestId;Action;DeadlineISO8601" + Environment.NewLine);
    }

    // Appends decision row; slaDue can be null (outputs "N/A" if so).
    public void AppendDecision(string requestId, string decision, string reason, DateTimeOffset? slaDue)
    {
        string due = slaDue.HasValue ? FormatIso(slaDue.Value) : "N/A";

        // Row format: RequestId;Decision;Reason;SlaDue.
        var line = string.Join(";",
            requestId,
            decision,
            reason,
            due
        );

        File.AppendAllText(decisionsFilePath, line + Environment.NewLine);
    }

    // Appends follow-up task row for meter upgrade or other actions.
    public void AppendFollowUp(string requestId, string action, DateTimeOffset deadline)
    {
        // Row format: RequestId;Action;Deadline.
        var line = string.Join(";",
            requestId,
            action,
            FormatIso(deadline)
        );

        File.AppendAllText(followUpsFilePath, line + Environment.NewLine);
    }

    private static string FormatIso(DateTimeOffset value) => value.ToString("yyyy-MM-ddTHH:mm:sszzz");
}
