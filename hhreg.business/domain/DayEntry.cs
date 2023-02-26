
namespace hhreg.business.domain;

public class DayEntry : BaseEntity<DayEntry>
{
    public long Id { get; set; }
    public string? Day { get; set; }
    public DayType DayType { get; set; } = DayType.Work;
    public string? Justification { get; set; }
    public double TotalMinutes { get; set; } = 0d; // in minutes
    public IEnumerable<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}