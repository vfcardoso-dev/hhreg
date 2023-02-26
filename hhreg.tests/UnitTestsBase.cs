using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using hhreg.business;
using NSubstitute;

namespace hhreg.tests;

public abstract class UnitTestsBase
{
    protected readonly Fixture Fixture = CreateFixture();
    protected readonly IAppSettings AppSettings;
    protected readonly LoggerStub Logger;

    public UnitTestsBase()
    {
        AppSettings = GetAppSettings();
        Logger = new LoggerStub();
    }

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

    private IAppSettings GetAppSettings()
    {
        var appSettings = Fixture.Create<AppSettings>();
        appSettings.DatabaseName = Guid.NewGuid().ToString().Split("-")[0];
        return appSettings;
    }
}