namespace Hhreg.Business.Utilities;

public static class DateAndTimeOnlyExtensions
{
    public static DateOnly ToDateOnly(this DateTime value)
    {
        return DateOnly.FromDateTime(value);
    }

    public static DateOnly ToDateOnly(this string value)
    {
        return DateOnly.Parse(value);
    }

    public static TimeOnly ToTimeOnly(this DateTime value)
    {
        return TimeOnly.FromDateTime(value);
    }

    public static TimeOnly ToTimeOnly(this string value)
    {
        return TimeOnly.Parse(value);
    }
}