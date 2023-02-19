public static class TimeExtensions
{
    public static string ToTimeString(this double value)
    {
        var signal = value < 0 ? "-" : "";
        return $"{signal}{TimeSpan.FromMinutes(value).ToString("hh\\:mm")}";
    }

    public static string ToTimeString(this TimeSpan value)
    {
        var signal = value < TimeSpan.Zero ? "-" : "";
        return $"{signal}{value.ToString("hh\\:mm")}";
    }

    public static string ToTimeString(this TimeOnly value)
    {
        return value.ToString("HH\\:mm");
    }
}