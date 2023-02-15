namespace hhreg.business.domain;

public class DayEntry
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public string? Justification { get; set; }
    public IList<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
