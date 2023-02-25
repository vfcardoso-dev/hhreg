using AutoFixture;
using AutoFixture.Kernel;
using hhreg.business.domain;

namespace hhreg.tests;

public class SettingsBuilder : BaseBuilder<Settings>
{
    public override Settings DoCreate(ISpecimenContext context)
    {
        var settings = new Settings
        {
            InitialBalance = context.Create<double>(),
            WorkDay = context.Create<double>(),
            StartCalculationsAt = context.Create<DateTime>().ToString("yyyy-MM-dd")
        };

        return settings;
    }
}