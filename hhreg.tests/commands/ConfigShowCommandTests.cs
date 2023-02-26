using AutoFixture;
using hhreg.business;
using hhreg.business.domain;
using NSubstitute;
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
        _sut = new ConfigShowCommand(_settingsRepository, Logger);
    }

    [Test]
    public void should_be_able_to_show_settings()
    {
        // Given
        var context = new CommandContext(_remainingArgs, "show", null);
        var settings = Fixture.Create<Settings>();
        _settingsRepository.Get().Returns(settings);

        // When
        var output = _sut!.Execute(context, new ConfigShowCommand.Settings());

        // Then
        output.Should().Be(0);
        Logger.MethodHits.Should().ContainSingle("WriteTable");
        
        var header = settings.ExtractColumns();
        Logger.Headers.Should().HaveCount(3);
        Logger.Headers.Should().Contain(header[0]);
        Logger.Headers.Should().Contain(header[1]);
        Logger.Headers.Should().Contain(header[2]);

        var row = settings.ExtractRow();
        Logger.Rows.Should().HaveCount(1);
        Logger.Rows.First().Should().HaveCount(3);
        Logger.Rows.First().Should().Contain(row[0]);
        Logger.Rows.First().Should().Contain(row[1]);
        Logger.Rows.First().Should().Contain(row[2]);
    }
}