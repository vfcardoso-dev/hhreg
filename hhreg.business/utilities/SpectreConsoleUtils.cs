using hhreg.business.domain;
using Spectre.Console;
using Spectre.Console.Rendering;

public static class SpectreConsoleUtils
{
    public static TableColumn[] GetDayEntrySummaryHeaders() 
    {
        var headers = new List<TableColumn>();
        headers.Add(new TableColumn(new Text("Day", new Style(Color.Blue, Color.Black))));
        headers.Add(new TableColumn(new Text("Day Type", new Style(Color.Blue, Color.Black))));
        headers.Add(new TableColumn(new Text("Time entries", new Style(Color.Blue, Color.Black))));
        headers.Add(new TableColumn(new Text("Total", new Style(Color.Blue, Color.Black))));
        headers.Add(new TableColumn(new Text("Balance", new Style(Color.Blue, Color.Black))));
        headers.Add(new TableColumn(new Text("Justification", new Style(Color.Blue, Color.Black))));
        return headers.ToArray();
    }

    public static TableColumn[] GetDayEntryBalanceHeaders() 
    {
        var headers = new List<TableColumn>();
        headers.Add(new TableColumn(new Text("Day", new Style(Color.Blue, Color.Black))));
        headers.Add(new TableColumn(new Text("Day Type", new Style(Color.Blue, Color.Black))));
        headers.Add(new TableColumn(new Text("Time entries / Justification", new Style(Color.Blue, Color.Black))));
        headers.Add(new TableColumn(new Text("Total", new Style(Color.Blue, Color.Black))));
        headers.Add(new TableColumn(new Text("Balance", new Style(Color.Blue, Color.Black))));
        headers.Add(new TableColumn(new Text("Accumulated", new Style(Color.Blue, Color.Black))));
        return headers.ToArray();
    }

    public static IRenderable[] GetDayEntrySummaryRow(DayEntry dayEntry, double workDay)
    {
        var workDayTs = TimeSpan.FromMinutes(workDay);
        var totalMinutesTs = TimeSpan.FromMinutes(dayEntry.TotalMinutes);
        var balance = totalMinutesTs.Subtract(workDayTs);
        var balanceColor = balance > TimeSpan.Zero ? Color.Green : Color.Red;
        var balanceSignal = balance > TimeSpan.Zero ? "+" : "";
        
        var row = new List<Text>();
        row.Add(new Text(DateOnly.Parse(dayEntry.Day!).ToString()));
        row.Add(new Text(dayEntry.DayType.ToString()));
        row.Add(new Text(string.Join(" / ", dayEntry.TimeEntries.Select(x => x.Time))));
        row.Add(new Text(totalMinutesTs.ToTimeString())); // Total hours
        row.Add(new Text($"{balanceSignal}{balance.ToTimeString()}", new Style(balanceColor, Color.Black))); // balance
        row.Add(new Text(dayEntry.Justification ?? "-")); // justification
        return row.ToArray();
    }

    public static IRenderable[] GetDayEntryBalanceRow(DayEntry dayEntry, double workDay, ref double accumulated)
    {
        var workDayTs = TimeSpan.FromMinutes(workDay);
        var totalMinutesTs = TimeSpan.FromMinutes(dayEntry.TotalMinutes);
        var balance = totalMinutesTs.Subtract(workDayTs);
        var balanceColor = balance >= TimeSpan.Zero ? Color.Green : Color.Red;
        var balanceSignal = balance > TimeSpan.Zero ? "+" : "";

        accumulated = accumulated + balance.TotalMinutes;
        var accumulatedTs = TimeSpan.FromMinutes(accumulated);
        var accumulatedColor = accumulatedTs > TimeSpan.Zero ? Color.Green : Color.Red;
        var accumulatedSignal = accumulatedTs > TimeSpan.Zero ? "+" : "";
        var timeEntries = dayEntry.TimeEntries.Any() 
            ? string.Join(" / ", dayEntry.TimeEntries.Select(x => x.Time)) 
            : dayEntry.Justification!;
        
        var row = new List<Text>();
        row.Add(new Text(DateOnly.Parse(dayEntry.Day!).ToString()));
        row.Add(new Text(dayEntry.DayType.ToString()));
        row.Add(new Text(timeEntries));
        row.Add(new Text(totalMinutesTs.ToTimeString())); // Total Minutes
        row.Add(new Text($"{balanceSignal}{balance.ToTimeString()}", new Style(balanceColor, Color.Black))); // balance
        row.Add(new Text($"{accumulatedSignal}{accumulatedTs.ToTimeString()}", new Style(accumulatedColor, Color.Black))); // balance

        return row.ToArray();
    }
}