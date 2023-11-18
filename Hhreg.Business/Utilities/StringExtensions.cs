using System.Text.RegularExpressions;

namespace Hhreg.Business.Utilities;

public static class StringExtensions
{
    private static readonly Regex TimeMatch = new("^-{0,1}(|[0-2])[0-9]\\:[0-5][0-9]$");
    private static readonly Regex IntegerMatch = new("^-{0,1}\\d{1,}$");

    public static bool IsTime(this string value)
    {
        return TimeMatch.IsMatch(value);
    }

    public static bool IsInteger(this string value)
    {
        return IntegerMatch.IsMatch(value);
    }

    public static bool IsEmpty(this string[] value)
    {
        return value == null || value.Any() == false;
    }
}