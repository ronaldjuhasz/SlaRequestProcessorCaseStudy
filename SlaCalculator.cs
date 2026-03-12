// Computes SLA due dates in Vienna timezone; handles DST edge cases (invalid times pushed forward).
public sealed class SlaCalculator
{
	private const int StandardSlaHours = 48;
	private const int PremiumSlaHours  = 24;
	private const int SmartMeterExtraHours = 12;
    private readonly TimeZoneInfo viennaTimeZone;

    // Constructor resolves Vienna timezone; may throw if IANA mapping not available on Windows.
    public SlaCalculator()
    {
        viennaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");
	}
    // Computes SLA due by adding hours in local Vienna time; tricky: handles DST gaps by pushing forward 1h.
    public DateTimeOffset ComputeSlaDue(DateTimeOffset requestedAt, string slaLevel, bool requiresMeterUpgrade)
	{
		int hours = slaLevel.Equals("Premium", StringComparison.OrdinalIgnoreCase) ? PremiumSlaHours : StandardSlaHours;

		// Add extra hours if meter upgrade is needed.
		if (requiresMeterUpgrade)
			hours += SmartMeterExtraHours;

		DateTime localRequested = TimeZoneInfo.ConvertTime(requestedAt, viennaTimeZone).DateTime;
		DateTime localDue = localRequested.AddHours(hours);

		// Tricky: if due falls in DST gap (invalid local time), push forward 1 hour.
		if (viennaTimeZone.IsInvalidTime(localDue))
		{
			localDue = localDue.AddHours(1);
		}

		TimeSpan offset = viennaTimeZone.GetUtcOffset(localDue);
		return new DateTimeOffset(localDue, offset);
	}

	// Computes upgrade deadline by subtracting the meter upgrade lead time from SLA due.
	public DateTimeOffset ComputeUpgradeDeadline(DateTimeOffset slaDue) => slaDue.AddHours(-SmartMeterExtraHours);

    // Formats DateTimeOffset as ISO8601 with timezone offset.
    public static string FormatIso(DateTimeOffset value) => value.ToString("yyyy-MM-ddTHH:mm:sszzz");
}
