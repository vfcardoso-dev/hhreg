using AutoFixture;
using AutoFixture.Kernel;
using hhreg.business.domain;

namespace hhreg.tests.infrastructure;

public class DayEntryBuilder : BaseBuilder<DayEntry>
{
    public override DayEntry DoCreate(ISpecimenContext context)
    {
        var dayEntryId = context.Create<long>();

        var dayEntry = new DayEntry
        {
            Id = dayEntryId,
            Day = context.Create<DateTime>().ToString("yyyy-MM-dd"),
            DayType = DayType.Work,
            Justification = null,
            TimeEntries = context.CreateMany<TimeEntry>(4)
                .OrderBy(x => x.Time)
                .Select(x => {
                    x.DayEntryId = dayEntryId;
                    return x;
                })
        };

        return dayEntry;
    }
}