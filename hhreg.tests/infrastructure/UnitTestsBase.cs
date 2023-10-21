using AutoFixture;
using hhreg.business.infrastructure;
using hhreg.tests.infrastructure.stubs;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace hhreg.tests.infrastructure;

public abstract class UnitTestsBase
{
    protected readonly Fixture Fixture = CreateFixture();
    protected readonly IDbSettings AppSettings;
    protected readonly LoggerStub Logger;
    protected ITypeRegistrar? TypeRegistrar;
    
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();

    public UnitTestsBase()
    {
        AppSettings = GetAppSettings();
        Logger = new LoggerStub();
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        InitializeTypeRegistrar();
    }

    [TearDown]
    public void TearDown()
    {
        _serviceCollection.Clear();
        InitializeTypeRegistrar();
        Logger.Clear();
    }

    private void InitializeTypeRegistrar()
    {
        TypeRegistrar = new TypeRegistrar(_serviceCollection);
    }

    protected void AddSingleton<T>() => _serviceCollection.AddSingleton(typeof(T));

    protected void AddSingleton<T1,T2>() => _serviceCollection.AddSingleton(typeof(T1), typeof(T2));

    protected void AddSingleton<T>(object instance) => _serviceCollection.AddSingleton(typeof(T), instance);

    protected ICommandApp CreateCommandApp(Action<IConfigurator> configurer)
    {
        var app = new CommandApp(TypeRegistrar);
        app.Configure(app => {
            app.PropagateExceptions();
            configurer(app);
        });
        return app;
    }

    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new DayEntryBuilder());
        fixture.Customizations.Add(new TimeEntryBuilder());
        fixture.Customizations.Add(new SettingsBuilder());
        return fixture;
    }

    private IDbSettings GetAppSettings()
    {
        var appSettings = Fixture.Create<SettingsService>();
        appSettings.DatabaseName = Guid.NewGuid().ToString().Split("-")[0];
        return appSettings;
    }
}