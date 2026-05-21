using AutoFixture;
using FluentAssertions;
using Hhreg.Business.Commands;
using Hhreg.Business.Domain;
using Hhreg.Business.Infrastructure;
using Hhreg.Tests.Infrastructure;
using NSubstitute;

namespace Hhreg.Tests.Commands;

public class ConfigShowCommandTests : UnitTestsBase
{
    private ISettingsService? _settingsService;

    [SetUp]
    public void ConfigShowCommandTests_SetUp()
    {
        _settingsService = Substitute.For<ISettingsService>();
    }

    [Test]
    public void Should_be_able_to_show_settings()
    {
        // Given
        AddSingleton<ISettingsService>(_settingsService!);
        AddSingleton<ILogger>(Logger);

        var settings = Fixture.Create<Settings>();
        _settingsService!.GetSettings().Returns(settings);

        var app = CreateCommandApp((config) => config.AddCommand<ConfigShowCommand>("show"));

        // When
        var output = app.Run(new[] { "show" });

        // Then
        output.Should().Be(0);
        Logger.MethodHits.Should().ContainSingle("WriteTable");

        var header = settings.ExtractColumns();
        Logger.Headers.Should().HaveCount(5);
        Logger.Headers.Should().Contain(header[0]);
        Logger.Headers.Should().Contain(header[1]);
        Logger.Headers.Should().Contain(header[2]);
        Logger.Headers.Should().Contain(header[3]);
        Logger.Headers.Should().Contain(header[4]);

        var row = settings.ExtractRow();
        Logger.Rows.Should().HaveCount(1);
        Logger.Rows.First().Should().HaveCount(5);
        Logger.Rows.First().Should().Contain(row[0]);
        Logger.Rows.First().Should().Contain(row[1]);
        Logger.Rows.First().Should().Contain(row[2]);
        Logger.Rows.First().Should().Contain(row[3]);
        Logger.Rows.First().Should().Contain(row[4]);
    }

    [Test]
    public void Should_format_monthly_bonus_percentage_with_percent_sign()
    {
        // Given
        AddSingleton<ISettingsService>(_settingsService!);
        AddSingleton<ILogger>(Logger);

        var settings = Fixture.Create<Settings>();
        settings.MonthlyBonusPercentage = 50;
        _settingsService!.GetSettings().Returns(settings);

        var app = CreateCommandApp((config) => config.AddCommand<ConfigShowCommand>("show"));

        // When
        var output = app.Run(new[] { "show" });

        // Then
        output.Should().Be(0);
        Logger.Headers.Should().Contain(nameof(Settings.MonthlyBonusPercentage));
        Logger.Rows.First().Should().Contain("50%");
    }
}