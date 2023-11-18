using AutoFixture;
using AutoFixture.Kernel;
using Hhreg.Business.Domain;

namespace Hhreg.Tests.Infrastructure;

public class SettingsBuilder : BaseBuilder<Settings>
{
    public override Settings DoCreate(ISpecimenContext context)
    {
        var settings = new Settings
        {
            StartBalanceInMinutes = context.Create<double>(),
            WorkDayInMinutes = context.Create<double>(),
            EntryToleranceInMinutes = 0,
            LastBalanceCutoff = context.Create<DateTime>().ToString("yyyy-MM-dd")
        };

        return settings;
    }
}