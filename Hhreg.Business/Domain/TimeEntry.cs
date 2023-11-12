using Hhreg.Business.Domain.Common;

namespace Hhreg.Business.Domain;

public class TimeEntry : BaseEntity<TimeEntry>
{
    public long Id { get; set; }
    public string? Time { get; set; }
    public long DayEntryId { get; set; }
}
