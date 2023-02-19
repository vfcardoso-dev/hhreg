using Spectre.Console;
using Spectre.Console.Rendering;

namespace hhreg.business.domain;

public class DayEntry : BaseEntity<DayEntry>
{
    public long Id { get; set; }
    public string? Day { get; set; }
    public DayType DayType { get; set; } = DayType.Work;
    public string? Justification { get; set; }
    public IList<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

    public override TableColumn[] RenderColumns() 
    {
        var headers = new List<TableColumn>();
        headers.Add(new TableColumn(new Text("Day", new Style(Color.Green, Color.Black))));
        headers.Add(new TableColumn(new Text("Day Type", new Style(Color.Green, Color.Black))));
        headers.Add(new TableColumn(new Text("Time entries", new Style(Color.Green, Color.Black))));
        headers.Add(new TableColumn(new Text("Justification", new Style(Color.Green, Color.Black))));
        return headers.ToArray();
    }

    public override IRenderable[] RenderRow()
    {
        var row = new List<Text>();
        row.Add(new Text(DateOnly.Parse(Day!).ToString()));
        row.Add(new Text(DayType.ToString()));
        row.Add(new Text(string.Join(" / ", TimeEntries.Select(x => x.Time))));
        row.Add(new Text(Justification ?? ""));
        return row.ToArray();
    }

    public TimeSpan SummarizeEntries(TimeSpan? currentBalance = null) {
        var summary = currentBalance ?? TimeSpan.Zero;

        for(int i = 0, j = 1; j < (TimeEntries.OrderBy(x => x.Time).Count()); i=j+1, j=j+2) {
            var second = TimeSpan.Parse(TimeEntries[j].Time!);
            var first = TimeSpan.Parse(TimeEntries[i].Time!);
            var diff = second.Subtract(first);
            summary = summary.Add(diff);
        }

        return summary;
    }

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

    public IRenderable[] RenderSummaryRow(double workDay, TimeSpan? currentBalance = null)
    {
        var summary = SummarizeEntries(currentBalance);
        var workDayTs = TimeSpan.FromMinutes(workDay);
        var balance = summary.Subtract(workDayTs);
        var balanceColor = balance > TimeSpan.Zero ? Color.Green : Color.Red;
        var plusSignal = balance > TimeSpan.Zero ? "+" : "";
        
        var row = new List<Text>();
        row.Add(new Text(DateOnly.Parse(Day!).ToString()));
        row.Add(new Text(DayType.ToString()));
        row.Add(new Text(string.Join(" / ", TimeEntries.Select(x => x.Time))));
        row.Add(new Text(summary.ToTimeString())); // Total hours
        row.Add(new Text($"{plusSignal}{balance.ToTimeString()}", new Style(balanceColor, Color.Black))); // balance
        row.Add(new Text(Justification ?? "-")); // justification
        return row.ToArray();
    }
}