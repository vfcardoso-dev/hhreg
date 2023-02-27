using System.Text.RegularExpressions;

namespace hhreg.business;

public static class StringExtensions
{
    private static readonly Regex TIME_MATCH = new("^-{0,1}(|[0-2])[0-9]\\:[0-5][0-9]$");
    private static readonly Regex INTEGER_MATCH = new("^-{0,1}\\d{1,}$");

    public static bool IsTime(this string value)
    {
        return TIME_MATCH.IsMatch(value);
    }

    public static bool IsInteger(this string value)
    {
        return INTEGER_MATCH.IsMatch(value);
    }
}