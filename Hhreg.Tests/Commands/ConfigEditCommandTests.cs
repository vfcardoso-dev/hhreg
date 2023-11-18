using AutoFixture;
using FluentAssertions;
using Hhreg.Business.Commands;
using Hhreg.Business.Domain;
using Hhreg.Business.Domain.ValueObjects;
using Hhreg.Business.Exceptions;
using Hhreg.Business.Infrastructure;
using Hhreg.Business.Utilities;
using Hhreg.Tests.Infrastructure;
using NSubstitute;

namespace Hhreg.Tests.Commands;

public class ConfigEditCommandTests : UnitTestsBase
{
    private ISettingsService? _settingsService;

    [SetUp]
    public void ConfigEditCommandTests_SetUp()
    {
        _settingsService = Substitute.For<ISettingsService>();
    }

    [TestCase(TimeInputMode.Hours, "70", HhregMessages.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Hours, "-10", HhregMessages.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Hours, "banana", HhregMessages.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Minutes, "banana", HhregMessages.CouldNotParseAsAValidIntegerFormat)]
    [TestCase(TimeInputMode.Minutes, "01:00", HhregMessages.CouldNotParseAsAValidIntegerFormat)]
    [TestCase(TimeInputMode.Minutes, "-01:00", HhregMessages.CouldNotParseAsAValidIntegerFormat)]
    public void Should_throw_validation_error_for_initial_balance_on_invalid_situations(TimeInputMode mode,
        string? initialBalance, string expectedValidationErrorMessage)
    {
        // Given
        AddSingleton<ISettingsService>(_settingsService!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<ConfigEditCommand>("edit"));

        // When
        Action action = () => app.Run(new[] { "edit", "-m", mode.ToString(), "-b", initialBalance! });

        // Then
        _settingsService!.DidNotReceive().SaveSettings(Arg.Any<Settings>());
        action.Should().Throw<Exception>().WithMessage(string.Format(expectedValidationErrorMessage, initialBalance));
    }

    [TestCase(TimeInputMode.Hours, "70", HhregMessages.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Hours, "-10", HhregMessages.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Hours, "banana", HhregMessages.CouldNotParseAsAValidTimeFormat)]
    [TestCase(TimeInputMode.Minutes, "banana", HhregMessages.CouldNotParseAsAValidIntegerFormat)]
    [TestCase(TimeInputMode.Minutes, "01:00", HhregMessages.CouldNotParseAsAValidIntegerFormat)]
    [TestCase(TimeInputMode.Minutes, "-01:00", HhregMessages.CouldNotParseAsAValidIntegerFormat)]
    public void Should_throw_validation_error_for_workday_on_invalid_situations(TimeInputMode mode,
        string? workDay, string expectedValidationErrorMessage)
    {
        // Given
        AddSingleton<ISettingsService>(_settingsService!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<ConfigEditCommand>("edit"));

        // When
        Action action = () => app.Run(new[] { "edit", "-m", mode.ToString(), "-w", workDay! });

        // Then
        _settingsService!.DidNotReceive().SaveSettings(Arg.Any<Settings>());
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
        AddSingleton<ISettingsService>(_settingsService!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<ConfigEditCommand>("edit"));

        // When
        Action action = () => app.Run(new[] { "edit", "-s", startCalculationsAt! });

        // Then
        _settingsService!.DidNotReceive().SaveSettings(Arg.Any<Settings>());

        var expectedValidationErrorMessage = string.Format(HhregMessages.CouldNotParseAsAValidDateFormat, startCalculationsAt);
        action.Should().Throw<Exception>().WithMessage(expectedValidationErrorMessage);
    }

    [TestCase(TimeInputMode.Hours)]
    [TestCase(TimeInputMode.Minutes)]
    public void Should_be_able_to_edit_initial_balance_setting(TimeInputMode mode)
    {
        // Given
        var signal = Fixture.Create<bool>() ? -1 : 1;
        var initialBalance = TimeSpan.FromMinutes(Math.Abs(Fixture.Create<int>()) * signal);

        AddSingleton<ISettingsService>(_settingsService!);
        AddSingleton<ILogger>(Logger);
        _settingsService!.GetSettings().Returns(Fixture.Create<Settings>());

        var app = CreateCommandApp((config) => config.AddCommand<ConfigEditCommand>("edit"));

        // When
        var result = app.Run(new[]{"edit",
            "-m", mode.ToString(),
            "-b", mode == TimeInputMode.Hours ? initialBalance.ToTimeString() : initialBalance.TotalMinutes.ToString()});

        // Then
        result.Should().Be(0);
        _settingsService!.Received(1).SaveSettings(Arg.Is<Settings>(x => x.StartBalanceInMinutes == initialBalance.TotalMinutes));
    }

    [TestCase(TimeInputMode.Hours)]
    [TestCase(TimeInputMode.Minutes)]
    public void Should_be_able_to_edit_workday_setting(TimeInputMode mode)
    {
        // Given
        var signal = Fixture.Create<bool>() ? -1 : 1;
        var workDay = TimeSpan.FromMinutes(Math.Abs(Fixture.Create<int>()) * signal);

        AddSingleton<ISettingsService>(_settingsService!);
        AddSingleton<ILogger>(Logger);
        _settingsService!.GetSettings().Returns(Fixture.Create<Settings>());

        var app = CreateCommandApp((config) => config.AddCommand<ConfigEditCommand>("edit"));

        // When
        var result = app.Run(new[]{"edit",
            "-m", mode.ToString(),
            "-w", mode == TimeInputMode.Hours ? workDay.ToTimeString() : workDay.TotalMinutes.ToString()});

        // Then
        result.Should().Be(0);
        _settingsService!.Received(1).SaveSettings(Arg.Is<Settings>(x => x.WorkDayInMinutes == workDay.TotalMinutes));
    }

    [Test]
    public void Should_be_able_to_edit_start_calculations_at_setting()
    {
        // Given
        var startCalculationsAt = DateOnly.FromDateTime(Fixture.Create<DateTime>());

        AddSingleton<ISettingsService>(_settingsService!);
        AddSingleton<ILogger>(Logger);
        _settingsService!.GetSettings().Returns(Fixture.Create<Settings>());

        var app = CreateCommandApp((config) => config.AddCommand<ConfigEditCommand>("edit"));

        // When
        var result = app.Run(new[]{"edit",
            "-s", startCalculationsAt.ToString("dd/MM/yyyy")});

        // Then
        result.Should().Be(0);
        _settingsService!.Received(1).SaveSettings(Arg.Is<Settings>(x => x.LastBalanceCutoff == startCalculationsAt.ToString("yyyy-MM-dd")));
    }
}