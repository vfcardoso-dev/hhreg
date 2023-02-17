namespace hhreg.business.domain;

public class DayEntry : BaseEntity<DayEntry>
{
    public Guid Id { get; set; }
    public DateOnly Day { get; set; }
    public string? Justification { get; set; }
    public IList<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
