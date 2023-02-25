using AutoFixture;

namespace hhreg.tests;

public abstract class UnitTestsBase
{
    protected readonly Fixture Fixture = CreateFixture();

    [OneTimeSetUp]
    public void Setup()
    {
    }

    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new DayEntryBuilder());
        fixture.Customizations.Add(new TimeEntryBuilder());
        fixture.Customizations.Add(new SettingsBuilder());
        return fixture;
    }
}