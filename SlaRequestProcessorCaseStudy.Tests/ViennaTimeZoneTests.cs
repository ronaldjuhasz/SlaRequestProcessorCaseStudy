// Verifies Vienna timezone DST handling (invalid/ambiguous times).
public class ViennaTimeZoneTests
{
    private static readonly TimeZoneInfo Vienna =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");

    [Fact]
    // Spring-forward gap: 02:30 on DST start does not exist.
    public void Vienna_InvalidLocalTime_DstStartGap_20260329_0230_IsInvalid()
    {
        // 2026-03-29 in Vienna: clocks jump from 02:00 to 03:00.
        // Therefore 02:30 does NOT exist.
        var local = new DateTime(2026, 3, 29, 2, 30, 0, DateTimeKind.Unspecified);

        Assert.True(Vienna.IsInvalidTime(local));
        Assert.False(Vienna.IsAmbiguousTime(local));
    }

    [Fact]
    // Fall-back overlap: 02:30 on DST end occurs twice (tricky: two valid offsets).
    public void Vienna_AmbiguousLocalTime_DstEndOverlap_20261025_0230_IsAmbiguous()
    {
        // 2026-10-25 in Vienna: clocks fall back (03:00 -> 02:00).
        // Therefore 02:30 occurs twice.
        var local = new DateTime(2026, 10, 25, 2, 30, 0, DateTimeKind.Unspecified);

        Assert.True(Vienna.IsAmbiguousTime(local));
        Assert.False(Vienna.IsInvalidTime(local));

        // Optional: show there are two valid offsets
        var offsets = Vienna.GetAmbiguousTimeOffsets(local);
        Assert.Equal(2, offsets.Length);
        Assert.Contains(TimeSpan.FromHours(2), offsets); // CEST
        Assert.Contains(TimeSpan.FromHours(1), offsets); // CET
    }
}