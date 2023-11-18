using Hhreg.Business.Domain;
using Spectre.Console;

namespace Hhreg.Business.Utilities;

public static class SpectreConsoleUtils
{
    public static string[] GetDayEntrySummaryHeaders()
    {
        return new string[]{
            "Data",
            "Tipo",
            "Marca��es",
            "Total",
            "Saldo",
            "Justificativa"
        };
    }

    public static string[] GetDayEntryBalanceHeaders()
    {
        return new string[]{
            "Data",
            "Tipo",
            "Marca��es / Justificativa",
            "Total",
            "Saldo",
            "Acumulado"
        };
    }

    public static Text[] GetDayEntrySummaryRow(DayEntry dayEntry, double workDay, Style? defaultRowStyle = null)
    {
        var workDayTs = TimeSpan.FromMinutes(workDay);
        var totalMinutesTs = TimeSpan.FromMinutes(dayEntry.TotalMinutes);
        var balance = totalMinutesTs.Subtract(workDayTs);
        var balanceColor = balance > TimeSpan.Zero ? Color.Green : Color.Red;
        var balanceSignal = balance > TimeSpan.Zero ? "+" : "";

        var evenTimeEntries = dayEntry.TimeEntries.Count() % 2 == 0;

        var totalMinutesResult = evenTimeEntries ? totalMinutesTs.ToTimeString() : "";
        var balanceResult = evenTimeEntries ? $"{balanceSignal}{balance.ToTimeString()}" : "";

        return new Text[]{
            new Text(dayEntry.Day!.ToDateOnly().ToString(), defaultRowStyle),
            new Text(dayEntry.DayType.ToString(), defaultRowStyle),
            new Text(string.Join(" / ", dayEntry.TimeEntries.Select(x => x.Time)), defaultRowStyle),
            new Text(totalMinutesResult, defaultRowStyle), // Total hours
            new Text(balanceResult, new Style(balanceColor, Color.Black)), // balance
            new Text(dayEntry.Justification ?? "-", defaultRowStyle) // justification
        };
    }

    public static Text[] GetDayEntryBalanceRow(DayEntry dayEntry, double workDay, ref double accumulated, Style? defaultRowStyle = null)
    {
        var workDayTs = TimeSpan.FromMinutes(workDay);
        var totalMinutesTs = TimeSpan.FromMinutes(dayEntry.TotalMinutes);
        var balance = totalMinutesTs.Subtract(workDayTs);
        var balanceColor = balance >= TimeSpan.Zero ? Color.Green : Color.Red;
        var balanceSignal = balance > TimeSpan.Zero ? "+" : "";

        var evenTimeEntries = dayEntry.TimeEntries.Count() % 2 == 0;

        accumulated += evenTimeEntries ? balance.TotalMinutes : 0;
        var accumulatedTs = TimeSpan.FromMinutes(accumulated);
        var accumulatedColor = accumulatedTs > TimeSpan.Zero ? Color.Green : Color.Red;
        var accumulatedSignal = accumulatedTs > TimeSpan.Zero ? "+" : "";
        var timeEntries = dayEntry.TimeEntries.Any()
            ? string.Join(" / ", dayEntry.TimeEntries.Select(x => x.Time))
            : dayEntry.Justification!;

        var totalMinutesResult = evenTimeEntries ? totalMinutesTs.ToTimeString() : "";
        var balanceResult = evenTimeEntries ? $"{balanceSignal}{balance.ToTimeString()}" : "";
        var accumulatedResult = evenTimeEntries ? $"{accumulatedSignal}{accumulatedTs.ToTimeString()}" : "";

        return new Text[] {
            new Text(dayEntry.Day!.ToDateOnly().ToString(), defaultRowStyle),
            new Text(dayEntry.DayType.ToString(), defaultRowStyle),
            new Text(timeEntries, defaultRowStyle),
            new Text(totalMinutesResult, defaultRowStyle), // Total Minutes
            new Text(balanceResult, new Style(balanceColor, Color.Black)), // balance
            new Text(accumulatedResult, new Style(accumulatedColor, Color.Black)) // balance
        };
    }
}