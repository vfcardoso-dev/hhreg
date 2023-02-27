using AutoFixture;
using AutoFixture.Kernel;
using hhreg.business;
using hhreg.business.domain;

namespace hhreg.tests;

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