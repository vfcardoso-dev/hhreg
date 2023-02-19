namespace hhreg.business.domain;

public class TimeEntry : BaseEntity<TimeEntry>
{
    public long Id { get; set; }
    public string? Time { get; set; }
    public long DayEntryId { get; set; }
}
