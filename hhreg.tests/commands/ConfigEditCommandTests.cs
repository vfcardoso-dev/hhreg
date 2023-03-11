using hhreg.business;
using NSubstitute;
using FluentAssertions;
using AutoFixture;

namespace hhreg.tests;

public class ConfigEditCommandTests : UnitTestsBase
{
    private ISettingsRepository? _settingsRepository;

    [SetUp]
    public void ConfigEditCommandTests_SetUp() {
        _settingsRepository = Substitute.For<ISettingsRepository>();
    }

    [TestCase(TimeInputMode.Hours, "70", HhregMessages.Common.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Hours, "-10", HhregMessages.Common.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Hours, "banana", HhregMessages.Common.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Minutes, "banana", HhregMessages.Common.CouldNotParseAsAValidIntegerFormat)]
    [TestCase(TimeInputMode.Minutes, "01:00", HhregMessages.Common.CouldNotParseAsAValidIntegerFormat)]
    [TestCase(TimeInputMode.Minutes, "-01:00", HhregMessages.Common.CouldNotParseAsAValidIntegerFormat)]
    public void Should_throw_validation_error_for_initial_balance_on_invalid_situations(TimeInputMode mode, 
        string? initialBalance, string expectedValidationErrorMessage)
    {
        // Given
        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<ConfigEditCommand>("edit"));

        // When
        Action action = () => app.Run(new []{"edit", "-m", mode.ToString(), "-b", initialBalance!});
        
        // Then
        _settingsRepository!.DidNotReceive().Update(Arg.Any<double?>(), Arg.Any<double?>(), Arg.Any<string?>());
        action.Should().Throw<Exception>().WithMessage(string.Format(expectedValidationErrorMessage, initialBalance));
    }

    [TestCase(TimeInputMode.Hours, "70", HhregMessages.Common.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Hours, "-10", HhregMessages.Common.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Hours, "banana", HhregMessages.Common.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Minutes, "banana", HhregMessages.Common.CouldNotParseAsAValidIntegerFormat)]
    [TestCase(TimeInputMode.Minutes, "01:00", HhregMessages.Common.CouldNotParseAsAValidIntegerFormat)]
    [TestCase(TimeInputMode.Minutes, "-01:00", HhregMessages.Common.CouldNotParseAsAValidIntegerFormat)]
    public void Should_throw_validation_error_for_workday_on_invalid_situations(TimeInputMode mode, 
        string? workDay, string expectedValidationErrorMessage)
    {
        // Given
        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<ConfigEditCommand>("edit"));

        // When
        Action action = () => app.Run(new []{"edit", "-m", mode.ToString(), "-w", workDay!});
        
        // Then
        _settingsRepository!.DidNotReceive().Update(Arg.Any<double?>(), Arg.Any<double?>(), Arg.Any<string?>());
        action.Should().Throw<Exception>().WithMessage(string.Format(expectedValidationErrorMessage, workDay));
    }

    [TestCase("70")]
    [TestCase("-10")]
    [TestCase("banana")]
    [TestCase("01:00")]
    [TestCase("-01:00")]
    public void Should_throw_validation_error_if_start_calculations_at_is_invalid(string? startCalculationsAt)
    {
        // Given
        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<ConfigEditCommand>("edit"));

        // When
        Action action = () => app.Run(new []{"edit", "-s", startCalculationsAt!});
        
        // Then
        _settingsRepository!.DidNotReceive().Update(Arg.Any<double?>(), Arg.Any<double?>(), Arg.Any<string?>());
        
        var expectedValidationErrorMessage = string.Format(HhregMessages.Common.CouldNotParseAsAValidDateFormat, startCalculationsAt);
        action.Should().Throw<Exception>().WithMessage(expectedValidationErrorMessage);
    }

    [TestCase(TimeInputMode.Hours)]
    [TestCase(TimeInputMode.Minutes)]
    public void Should_be_able_to_edit_initial_balance_setting(TimeInputMode mode)
    {
        // Given
        var signal = Fixture.Create<bool>() ? -1 : 1;
        var initialBalance = TimeSpan.FromMinutes(Math.Abs(Fixture.Create<int>()) * signal);

        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<ConfigEditCommand>("edit"));

        // When
        var result = app.Run(new []{"edit", 
            "-m", mode.ToString(),
            "-b", mode == TimeInputMode.Hours ? initialBalance.ToTimeString() : initialBalance.TotalMinutes.ToString()});
        
        // Then
        result.Should().Be(0);
        _settingsRepository!.Received(1).Update(initialBalance.TotalMinutes, null, null);
    }

    [TestCase(TimeInputMode.Hours)]
    [TestCase(TimeInputMode.Minutes)]
    public void Should_be_able_to_edit_workday_setting(TimeInputMode mode)
    {
        // Given
        var signal = Fixture.Create<bool>() ? -1 : 1;
        var workDay = TimeSpan.FromMinutes(Math.Abs(Fixture.Create<int>()) * signal);

        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<ConfigEditCommand>("edit"));

        // When
        var result = app.Run(new []{"edit", 
            "-m", mode.ToString(),
            "-w", mode == TimeInputMode.Hours ? workDay.ToTimeString() : workDay.TotalMinutes.ToString()});
        
        // Then
        result.Should().Be(0);
        _settingsRepository!.Received(1).Update(null, workDay.TotalMinutes, null);
    }

    [Test]
    public void Should_be_able_to_edit_start_calculations_at_setting()
    {
        // Given
        var startCalculationsAt = DateOnly.FromDateTime(Fixture.Create<DateTime>());

        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<ConfigEditCommand>("edit"));

        // When
        var result = app.Run(new []{"edit", 
            "-s", startCalculationsAt.ToString("dd/MM/yyyy")});
        
        // Then
        result.Should().Be(0);
        _settingsRepository!.Received(1).Update(null, null, startCalculationsAt.ToString("yyyy-MM-dd"));
    }
}