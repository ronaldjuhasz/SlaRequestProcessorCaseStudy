namespace SlaRequestProcessorCaseStudy.Tests;

// Verifies SLA time calculations around DST and upgrade rules.
public class SlaCalculatorTests
{
    private readonly SlaCalculator sla = new();

    [Fact]
    // Standard SLA across DST start keeps local wall-clock semantics.
    public void ComputeSlaDue_Standard_AcrossDstStart_UsesLocalWallClockSemantics()
    {
        // DST start edge in Vienna.
        var requestedAt = new DateTimeOffset(2025, 3, 30, 1, 30, 0, TimeSpan.FromHours(1));

        var due = sla.ComputeSlaDue(requestedAt, "Standard", requiresMeterUpgrade: false);

        Assert.Equal(2025, due.Year);
        Assert.Equal(4, due.Month);
        Assert.Equal(1, due.Day);
        Assert.Equal(1, due.Hour);
        Assert.Equal(30, due.Minute);

        // Offset should switch to summer time.
        Assert.Equal(TimeSpan.FromHours(2), due.Offset);
    }

    [Fact]
    // Standard SLA across DST end keeps local wall-clock semantics.
    public void ComputeSlaDue_Standard_AcrossDstEnd_UsesLocalWallClockSemantics()
    {
        // DST end edge in Vienna.
        var requestedAt = new DateTimeOffset(2025, 10, 26, 1, 30, 0, TimeSpan.FromHours(2));

        var due = sla.ComputeSlaDue(requestedAt, "Standard", requiresMeterUpgrade: false);

        Assert.Equal(2025, due.Year);
        Assert.Equal(10, due.Month);
        Assert.Equal(28, due.Day);
        Assert.Equal(1, due.Hour);
        Assert.Equal(30, due.Minute);

        // Offset should switch back to winter time.
        Assert.Equal(TimeSpan.FromHours(1), due.Offset);
    }

    [Fact]
    // Meter upgrade adds 12h and deadline is due minus 12h.
    public void ComputeSlaDue_Premium_WithMeterUpgrade_Adds12Hours_AndUpgradeDeadlineIsBaseSlaDue()
    {
        // Non-DST-boundary date for simpler assertion.
        var requestedAt = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.FromHours(2));

        var dueWithUpgrade = sla.ComputeSlaDue(requestedAt, "Premium", requiresMeterUpgrade: true);
        var upgradeDeadline = sla.ComputeUpgradeDeadline(dueWithUpgrade);

        Assert.Equal(new DateTimeOffset(2025, 6, 16, 22, 0, 0, TimeSpan.FromHours(2)), dueWithUpgrade);

        // Tricky part: deadline is before due by the upgrade lead time.
        Assert.Equal(new DateTimeOffset(2025, 6, 16, 10, 0, 0, TimeSpan.FromHours(2)), upgradeDeadline);
    }

	[Fact]
    // Invalid local time in DST gap is pushed to next valid time.
    public void ComputeSlaDue_WhenLocalDueFallsIntoInvalidGap_PushesForwardToValidTime()
    {
        // 2026-03-29 02:30 in Vienna is invalid (spring-forward gap).
        var requestedAt = new DateTimeOffset(2026, 3, 28, 2, 30, 0, TimeSpan.FromHours(1));

        var due = sla.ComputeSlaDue(requestedAt, "Premium", requiresMeterUpgrade: false);

        Assert.Equal(2026, due.Year);
        Assert.Equal(3, due.Month);
        Assert.Equal(29, due.Day);
        Assert.Equal(3, due.Hour);
        Assert.Equal(30, due.Minute);
        Assert.Equal(TimeSpan.FromHours(2), due.Offset);
    }
}
