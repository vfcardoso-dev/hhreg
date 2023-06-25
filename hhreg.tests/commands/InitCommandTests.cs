using AutoFixture;
using FluentAssertions;
using hhreg.business.commands;
using hhreg.business.domain.valueObjects;
using hhreg.business.exceptions;
using hhreg.business.infrastructure;
using hhreg.business.repositories;
using hhreg.business.utilities;
using hhreg.tests.infrastructure;
using NSubstitute;

namespace hhreg.tests.commands;

public class InitCommandTests : UnitTestsBase
{
    private ISettingsRepository? _settingsRepository;

    [SetUp]
    public void InitCommandTests_SetUp() {
        _settingsRepository = Substitute.For<ISettingsRepository>();
    }

    [Test]
    public void deve_falhar_se_nao_for_informado_initial_balance(){
        // Given
        var initialBalanceInMinutes = Fixture.Create<int>();
        var workDayInMinutes = Fixture.Create<int>();
        var initialBalance = TimeSpan.FromMinutes(initialBalanceInMinutes).ToTimeString();
        var workDay = TimeSpan.FromMinutes(workDayInMinutes).ToTimeString();
        var startCalculationsAt = Fixture.Create<DateTime>();

        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        _settingsRepository!.IsAlreadyInitialized().Returns(false);

        var app = CreateCommandApp((config) => config.AddCommand<InitCommand>("init"));

        // When
        Action action = () => app.Run(new []{"init", "-w", workDay, "-s", startCalculationsAt!.ToString("dd/MM/yyyy")});
        
        // Then
        _settingsRepository!.DidNotReceive().Create(initialBalanceInMinutes, workDayInMinutes, startCalculationsAt.ToString("yyyy-MM-dd"));
        action.Should().Throw<Exception>().WithMessage(HhregMessages.YouShouldInformInitialBalance);
    }

    [Test]
    public void deve_falhar_se_nao_for_informado_workday()
    {
        // Given
        var initialBalanceInMinutes = Fixture.Create<int>();
        var workDayInMinutes = Fixture.Create<int>();
        var initialBalance = TimeSpan.FromMinutes(initialBalanceInMinutes).ToTimeString();
        var workDay = TimeSpan.FromMinutes(workDayInMinutes).ToTimeString();
        var startCalculationsAt = Fixture.Create<DateTime>();

        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        _settingsRepository!.IsAlreadyInitialized().Returns(false);

        var app = CreateCommandApp((config) => config.AddCommand<InitCommand>("init"));

        // When
        Action action = () => app.Run(new []{"init", "-b", initialBalance, "-s", startCalculationsAt!.ToString("dd/MM/yyyy")});
        
        // Then
        _settingsRepository!.DidNotReceive().Create(initialBalanceInMinutes, workDayInMinutes, startCalculationsAt.ToString("yyyy-MM-dd"));
        action.Should().Throw<Exception>().WithMessage(HhregMessages.YouShouldInformWorkday);
    }

    [Test]
    public void deve_falhar_se_nao_for_informado_start_calculations_at()
    {
        // Given
        var initialBalanceInMinutes = Fixture.Create<int>();
        var workDayInMinutes = Fixture.Create<int>();
        var initialBalance = TimeSpan.FromMinutes(initialBalanceInMinutes).ToTimeString();
        var workDay = TimeSpan.FromMinutes(workDayInMinutes).ToTimeString();
        var startCalculationsAt = Fixture.Create<DateTime>();

        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        _settingsRepository!.IsAlreadyInitialized().Returns(false);

        var app = CreateCommandApp((config) => config.AddCommand<InitCommand>("init"));

        // When
        Action action = () => app.Run(new []{"init", "-b", initialBalance, "-w", workDay});
        
        // Then
        _settingsRepository!.DidNotReceive().Create(initialBalanceInMinutes, workDayInMinutes, startCalculationsAt.ToString("yyyy-MM-dd"));
        action.Should().Throw<Exception>().WithMessage(HhregMessages.YouShouldInformStartCalculationsAt);
    }

    [TestCase(TimeInputMode.Hours, "70", HhregMessages.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Hours, "-10", HhregMessages.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Hours, "banana", HhregMessages.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Minutes, "banana", HhregMessages.CouldNotParseAsAValidIntegerFormat)]
    [TestCase(TimeInputMode.Minutes, "01:00", HhregMessages.CouldNotParseAsAValidIntegerFormat)]
    [TestCase(TimeInputMode.Minutes, "-01:00", HhregMessages.CouldNotParseAsAValidIntegerFormat)]
    public void deve_falhar_valor_informado_para_initial_balance_for_invalido(
        TimeInputMode mode, string initialBalance, string expectedExceptionMessage){
        // Given
        var workDayInMinutes = Fixture.Create<int>();
        var workDay = TimeSpan.FromMinutes(workDayInMinutes).ToTimeString();
        var workdayParam = mode == TimeInputMode.Hours ? workDay : workDayInMinutes.ToString();
        var startCalculationsAt = Fixture.Create<DateTime>();

        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        _settingsRepository!.IsAlreadyInitialized().Returns(false);

        var app = CreateCommandApp((config) => config.AddCommand<InitCommand>("init"));

        // When
        Action action = () => app.Run(new []{"init", "-m", mode.ToString(), "-b", initialBalance, 
            "-w", workdayParam, "-s", startCalculationsAt!.ToString("dd/MM/yyyy")});
        
        // Then
        _settingsRepository!.DidNotReceive().Create(Arg.Any<double>(), workDayInMinutes, startCalculationsAt.ToString("yyyy-MM-dd"));
        action.Should().Throw<Exception>().WithMessage(string.Format(expectedExceptionMessage, initialBalance));
    }

    [TestCase(TimeInputMode.Hours, "70", HhregMessages.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Hours, "-10", HhregMessages.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Hours, "banana", HhregMessages.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Minutes, "banana", HhregMessages.CouldNotParseAsAValidIntegerFormat)]
    [TestCase(TimeInputMode.Minutes, "01:00", HhregMessages.CouldNotParseAsAValidIntegerFormat)]
    [TestCase(TimeInputMode.Minutes, "-01:00", HhregMessages.CouldNotParseAsAValidIntegerFormat)]
    public void deve_falhar_valor_informado_para_workday_for_invalido(
        TimeInputMode mode, string workday, string expectedExceptionMessage){
        // Given
        var initialBalanceInMinutes = Fixture.Create<int>();
        var initialBalance = TimeSpan.FromMinutes(initialBalanceInMinutes).ToTimeString();
        var initialBalanceParam = mode == TimeInputMode.Hours ? initialBalance : initialBalanceInMinutes.ToString();
        var startCalculationsAt = Fixture.Create<DateTime>();

        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        _settingsRepository!.IsAlreadyInitialized().Returns(false);

        var app = CreateCommandApp((config) => config.AddCommand<InitCommand>("init"));

        // When
        Action action = () => app.Run(new []{"init", "-m", mode.ToString(), "-b", initialBalanceParam, 
            "-w", workday, "-s", startCalculationsAt!.ToString("dd/MM/yyyy")});
        
        // Then
        _settingsRepository!.DidNotReceive().Create(initialBalanceInMinutes, Arg.Any<double>(), startCalculationsAt.ToString("yyyy-MM-dd"));
        action.Should().Throw<Exception>().WithMessage(string.Format(expectedExceptionMessage, workday));
    }

    [TestCase("70")]
    [TestCase("-10")]
    [TestCase("banana")]
    [TestCase("01:00")]
    [TestCase("-01:00")]
    public void deve_falhar_valor_informado_para_start_calculations_at_for_invalido(string startCalculationsAt)
    {
        // Given
        var initialBalanceInMinutes = Fixture.Create<int>();
        var workdayInMinutes = Fixture.Create<int>();

        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        _settingsRepository!.IsAlreadyInitialized().Returns(false);

        var app = CreateCommandApp((config) => config.AddCommand<InitCommand>("init"));

        // When
        Action action = () => app.Run(new []{"init", "-m", "Minutes", "-b", initialBalanceInMinutes.ToString(), 
            "-w", workdayInMinutes.ToString(), "-s", startCalculationsAt});
        
        // Then
        _settingsRepository!.DidNotReceive().Create(initialBalanceInMinutes, workdayInMinutes, startCalculationsAt);
        action.Should().Throw<Exception>().WithMessage(string.Format(HhregMessages.CouldNotParseAsAValidDateFormat, startCalculationsAt));   
    }

    [Test]
    public void deve_falhar_se_configuracao_inicial_ja_tiver_sido_feita()
    {
        // Given
        var initialBalanceInMinutes = Fixture.Create<int>();
        var workdayInMinutes = Fixture.Create<int>();
        var startCalculationsAt = Fixture.Create<DateTime>().ToString("dd/MM/yyyy");

        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        _settingsRepository!.IsAlreadyInitialized().Returns(true);

        var app = CreateCommandApp((config) => config.AddCommand<InitCommand>("init"));

        // When
        Action action = () => app.Run(new []{"init", "-m", "Minutes", "-b", initialBalanceInMinutes.ToString(), 
            "-w", workdayInMinutes.ToString(), "-s", startCalculationsAt});
        
        // Then
        _settingsRepository!.DidNotReceive().Create(initialBalanceInMinutes, workdayInMinutes, startCalculationsAt);
        action.Should().Throw<Exception>().WithMessage(HhregMessages.SettingsAlreadyInitialized);   
    }

    [Test]
    public void deve_ser_capaz_de_inicializar_as_configuracoes_em_minutos()
    {
        // Given
        var initialBalanceInMinutes = Fixture.Create<int>();
        var workdayInMinutes = Fixture.Create<int>();
        var startCalculationsAt = Fixture.Create<DateTime>();

        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        _settingsRepository!.IsAlreadyInitialized().Returns(false);

        var app = CreateCommandApp((config) => config.AddCommand<InitCommand>("init"));

        // When
        var output = app.Run(new []{"init", "-m", "Minutes", "-b", initialBalanceInMinutes.ToString(), 
            "-w", workdayInMinutes.ToString(), "-s", startCalculationsAt.ToString("dd/MM/yyyy")});
        
        // Then
        output.Should().Be(0);
        _settingsRepository!.Received().Create(initialBalanceInMinutes, workdayInMinutes, startCalculationsAt.ToString("yyyy-MM-dd"));
        Logger.MethodHits.Where(x => x == "WriteLine").Should().HaveCount(1);
        Logger.Lines.Should().Contain("Settings [green]SUCCESSFULLY[/] initialized!");
    }

    [Test]
    public void deve_ser_capaz_de_inicializar_as_configuracoes_em_horas()
    {
        // Given
        var initialBalanceInMinutes = Fixture.Create<int>();
        var initialBalance = TimeSpan.FromMinutes(initialBalanceInMinutes).ToTimeString();
        var workdayInMinutes = Fixture.Create<int>();
        var workday = TimeSpan.FromMinutes(workdayInMinutes).ToTimeString();
        var startCalculationsAt = Fixture.Create<DateTime>();

        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        _settingsRepository!.IsAlreadyInitialized().Returns(false);

        var app = CreateCommandApp((config) => config.AddCommand<InitCommand>("init"));

        // When
        var output = app.Run(new []{"init", "-m", "Hours", "-b", initialBalance, "-w", workday, "-s", startCalculationsAt.ToString("dd/MM/yyyy")});
        
        // Then
        output.Should().Be(0);
        _settingsRepository!.Received().Create(initialBalanceInMinutes, workdayInMinutes, startCalculationsAt.ToString("yyyy-MM-dd"));
        Logger.MethodHits.Where(x => x == "WriteLine").Should().HaveCount(1);
        Logger.Lines.Should().Contain("Settings [green]SUCCESSFULLY[/] initialized!");
    }
}