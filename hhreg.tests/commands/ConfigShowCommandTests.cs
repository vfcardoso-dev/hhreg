using AutoFixture;
using hhreg.business;
using hhreg.business.domain;
using NSubstitute;
using FluentAssertions;

namespace hhreg.tests;

public class ConfigShowCommandTests : UnitTestsBase
{
    private ISettingsRepository? _settingsRepository;

    [SetUp]
    public void ConfigShowCommandTests_SetUp() {
        _settingsRepository = Substitute.For<ISettingsRepository>();
    }

    [Test]
    public void Should_be_able_to_show_settings()
    {
        // Given
        AddSingleton<ISettingsRepository>(_settingsRepository!);
        AddSingleton<ILogger>(Logger);

        var settings = Fixture.Create<Settings>();
        _settingsRepository!.Get().Returns(settings);

        var app = CreateCommandApp((config) => config.AddCommand<ConfigShowCommand>("show"));

        // When
        var output = app.Run(new []{"show"});

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