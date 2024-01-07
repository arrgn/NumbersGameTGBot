static class TimePeriod
{
    public static DateTime MinTime;
    public static DateTime MaxTime;
    public static TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
}