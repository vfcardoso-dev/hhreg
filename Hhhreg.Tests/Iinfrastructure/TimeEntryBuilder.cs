using AutoFixture;
using AutoFixture.Kernel;
using hhreg.business.domain;
using hhreg.business.utilities;

namespace hhreg.tests.infrastructure;

public class TimeEntryBuilder : BaseBuilder<TimeEntry>
{
    public override TimeEntry DoCreate(ISpecimenContext context)
    {
        var timeEntry = new TimeEntry
        {
            Id = context.Create<long>(),
            DayEntryId = context.Create<long>(),
            Time = TimeOnly.FromDateTime(context.Create<DateTime>()).ToTimeString()
        };

        return timeEntry;
    }
}