using Spectre.Console;
using Spectre.Console.Rendering;

namespace hhreg.business.domain;

public class DayEntry : BaseEntity<DayEntry>
{
    public long Id { get; set; }
    public string? Day { get; set; }
    public DayType DayType { get; set; } = DayType.Work;
    public string? Justification { get; set; }
    public double TotalHours { get; set; } = 0d; // in minutes
    public IEnumerable<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

    public TableColumn[] RenderSummaryHeaders() 
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

    public IRenderable[] RenderSummaryRow(double workDay)
    {
        var workDayTs = TimeSpan.FromMinutes(workDay);
        var totalHoursTs = TimeSpan.FromMinutes(TotalHours);
        var balance = totalHoursTs.Subtract(workDayTs);
        var balanceColor = balance > TimeSpan.Zero ? Color.Green : Color.Red;
        var plusSignal = balance > TimeSpan.Zero ? "+" : "";
        
        var row = new List<Text>();
        row.Add(new Text(DateOnly.Parse(Day!).ToString()));
        row.Add(new Text(DayType.ToString()));
        row.Add(new Text(string.Join(" / ", TimeEntries.Select(x => x.Time))));
        row.Add(new Text(totalHoursTs.ToTimeString())); // Total hours
        row.Add(new Text($"{plusSignal}{balance.ToTimeString()}", new Style(balanceColor, Color.Black))); // balance
        row.Add(new Text(Justification ?? "-")); // justification
        return row.ToArray();
    }
}