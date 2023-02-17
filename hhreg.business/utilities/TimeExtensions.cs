public static class TimeExtensions
{
    public static string ToTimeString(this double value)
    {
        var signal = value < 0 ? "-" : "";
        return $"{signal}{TimeSpan.FromMinutes(value).ToString("hh\\:mm")}";
    }
}