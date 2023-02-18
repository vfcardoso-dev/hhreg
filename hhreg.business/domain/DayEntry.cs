namespace hhreg.business.domain;

public class DayEntry : BaseEntity<DayEntry>
{
    public long Id { get; set; }
    public string? Day { get; set; }
    public string? Justification { get; set; }
    public IList<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
