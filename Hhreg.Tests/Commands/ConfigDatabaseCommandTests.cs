using FluentAssertions;
using Hhreg.Business.Commands;
using Hhreg.Business.Exceptions;
using Hhreg.Business.Infrastructure;
using Hhreg.Tests.Infrastructure;

namespace Hhreg.Tests.Commands;

public class ConfigDatabaseCommandTests : UnitTestsBase
{
    [Test]
    public void Should_be_able_to_show_database_location()
    {
        // Given
        AddSingleton<ISettingsService>(AppSettings);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<ConfigDatabaseCommand>("database"));

        // When
        var output = app.Run(new[] { "database" });

        // Then
        output.Should().Be(0);
        Logger.MethodHits.Should().ContainSingle("WriteFilePath");
        Logger.Headers.Should().ContainSingle(HhregMessages.DatabaseLocationTitle);
        Logger.Rows.Should().HaveCount(1);
        Logger.Rows.First().Should().ContainSingle(AppSettings.DatabaseFilePath);
    }
}