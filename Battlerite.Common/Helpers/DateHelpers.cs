using System;

public static class DateHelpers
{
    private static readonly DateTime OriginUtc = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToUniversalTime();

    public static DateTime DateTimeFromUnixTime(ulong unixTime)
    {
        return OriginUtc.AddSeconds((double)unixTime).ToLocalTime();
    }

    public static ulong DateTimeToUnixTime(DateTime time)
    {
        return (ulong)time.ToUniversalTime().Subtract(OriginUtc).TotalSeconds;
    }

    public static ulong DateTimeToUnixTimeMs(DateTime time)
    {
        return (ulong)time.ToUniversalTime().Subtract(OriginUtc).TotalMilliseconds;
    }

    public static string GetElapsed(TimeSpan span)
    {
        if (span.Hours > 0)
        {
            return $"{span.Hours:00}h:{span.Minutes:00}m:{span.Seconds:00}s";
        }
        if (span.Minutes > 0)
        {
            return $"{span.Minutes:00}m:{span.Seconds:00}s";
        }
        if (span.Seconds > 0)
        {
            return $"{span.Seconds:00}s";
        }
        return string.Empty;
    }
}
