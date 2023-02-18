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

    public override TableColumn[] CreateHeaders() 
    {
        var headers = new List<TableColumn>();
        headers.Add(new TableColumn(new Text("Day", new Style(Color.Green, Color.Black))));
        headers.Add(new TableColumn(new Text("Day Type", new Style(Color.Green, Color.Black))));
        headers.Add(new TableColumn(new Text("Time entries", new Style(Color.Green, Color.Black))));
        headers.Add(new TableColumn(new Text("Justification", new Style(Color.Green, Color.Black))));
        return headers.ToArray();
    }

    public override IRenderable[] CreateRenderableRow()
    {
        var row = new List<Text>();
        row.Add(new Text(DateOnly.Parse(Day!).ToString()));
        row.Add(new Text(DayType.ToString()));
        row.Add(new Text(string.Join(", ", TimeEntries.Select(x => x.Time))));
        row.Add(new Text(Justification ?? ""));

        return row.ToArray();
    }
}