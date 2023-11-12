using FluentAssertions;
using hhreg.business.commands;
using hhreg.business.exceptions;
using hhreg.business.infrastructure;
using hhreg.tests.infrastructure;

namespace hhreg.tests.commands;

public class ConfigDatabaseCommandTests : UnitTestsBase
{
    [Test]
    public void Should_be_able_to_show_database_location()
    {
        // Given
        AddSingleton<IDbSettings>(AppSettings);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<ConfigDatabaseCommand>("database"));

        // When
        var output = app.Run(new []{"database"});

        // Then
        output.Should().Be(0);
        Logger.MethodHits.Should().ContainSingle("WriteFilePath");
        Logger.Headers.Should().ContainSingle(HhregMessages.DatabaseLocationTitle);
        Logger.Rows.Should().HaveCount(1);
        Logger.Rows.First().Should().ContainSingle(AppSettings.DatabaseFilePath);
    }
}