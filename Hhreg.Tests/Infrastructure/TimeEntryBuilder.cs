using AutoFixture;
using AutoFixture.Kernel;
using Hhreg.Business.Domain;
using Hhreg.Business.Utilities;

namespace Hhreg.Tests.Infrastructure;

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