using hhreg.business;
using NSubstitute;
using Spectre.Console.Cli;
using FluentAssertions;

namespace hhreg.tests;

public class ConfigDatabaseCommandTests : UnitTestsBase
{
    private readonly IRemainingArguments _remainingArgs = Substitute.For<IRemainingArguments>();
    private ConfigDatabaseCommand? _sut;

    [SetUp]
    public void ConfigDatabaseCommand_SetUp() {
        _sut = new ConfigDatabaseCommand(AppSettings, Logger);
    }

    [Test]
    public void should_be_able_to_show_database_location()
    {
        // Given
        var context = new CommandContext(_remainingArgs, "database", null);

        // When
        var output = _sut!.Execute(context, new ConfigDatabaseCommand.Settings());

        // Then
        output.Should().Be(0);
        Logger.MethodHits.Should().ContainSingle("WriteFilePath");
        Logger.Headers.Should().ContainSingle("Database location");
        Logger.Rows.Should().HaveCount(1);
        Logger.Rows.First().Should().ContainSingle(AppSettings.DatabaseFilePath);
    }
}