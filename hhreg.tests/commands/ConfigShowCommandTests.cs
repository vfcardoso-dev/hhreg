using AutoFixture;
using hhreg.business;
using hhreg.business.domain;
using NSubstitute;
using Spectre.Console;
using Spectre.Console.Cli;
using FluentAssertions;

namespace hhreg.tests;

public class ConfigShowCommandTests : UnitTestsBase
{
    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly IRemainingArguments _remainingArgs = Substitute.For<IRemainingArguments>();
    private ConfigShowCommand? _sut;

    [SetUp]
    public void ConfigShowCommandTests_SetUp() {
        _sut = new ConfigShowCommand(_settingsRepository);
    }

    [Test]
    public void should_be_able_to_show_settings()
    {
        // Given
        var context = new CommandContext(_remainingArgs, "show", null);
        var settings = Fixture.Create<Settings>();
        _settingsRepository.Get().Returns(settings);
        AnsiConsole.Record();

        // When
        var output = _sut!.Execute(context, new ConfigShowCommand.Settings());
        var consoleExport = AnsiConsole.ExportText();

        // Then
        output.Should().Be(0);
        consoleExport.Should().Contain("InitialBalance");
        consoleExport.Should().Contain(TimeSpan.FromMinutes(settings.InitialBalance).ToTimeString());
        consoleExport.Should().Contain("WorkDay");
        consoleExport.Should().Contain(TimeSpan.FromMinutes(settings.WorkDay).ToTimeString());
        consoleExport.Should().Contain("StartCalculationsAt");
        consoleExport.Should().Contain(DateOnly.Parse(settings.StartCalculationsAt).ToString("dd/MM/yyyy"));
    }
}