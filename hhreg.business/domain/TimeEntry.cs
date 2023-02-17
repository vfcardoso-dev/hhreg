namespace hhreg.business.domain;

public class TimeEntry : BaseEntity<TimeEntry>
{
    public Guid Id { get; set; }
    public TimeOnly Time { get; set; }
}
