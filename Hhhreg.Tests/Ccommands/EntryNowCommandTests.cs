using AutoFixture;
using FluentAssertions;
using hhreg.business.commands;
using hhreg.business.domain;
using hhreg.business.infrastructure;
using hhreg.business.repositories;
using hhreg.business.utilities;
using hhreg.tests.infrastructure;
using NSubstitute;

namespace hhreg.tests.commands;

public class EntryNowCommandTests : UnitTestsBase
{
    private ITimeRepository? _timeRepository;

    [SetUp]
    public void EntryNowCommandTests_SetUp() {
        _timeRepository = Substitute.For<ITimeRepository>();
    }
    
    [Test]
    public void deve_ser_capaz_de_lancar_entradas()
    {
        // Given
        var today = DateTime.Today.ToDateOnly();
        var now = TimeOnly.FromDateTime(DateTime.Now).ToTimeString();
        var dayEntry = Fixture.Create<DayEntry>();
        AddSingleton<ITimeRepository>(_timeRepository!);
        AddSingleton<ILogger>(Logger);

        dayEntry.Day = today.ToString("yyyy/MM/dd");

        _timeRepository!.GetOrCreateDay(Arg.Any<DateOnly>()).Returns(dayEntry);

        var app = CreateCommandApp((config) => config.AddCommand<EntryNowCommand>("now"));

        // When
        var output = app.Run(new []{"now"});
        
        // Then
        output.Should().Be(0);
        _timeRepository!.Received().GetOrCreateDay(today);
        _timeRepository!.Received().CreateTime(dayEntry.Id, Arg.Is<string>(x => x == now));
        Logger.MethodHits.Where(x => x == "WriteLine").Should().HaveCount(2);
        Logger.Lines.Should().Contain("Day entry [green]SUCCESSFULLY[/] created!");
        Logger.Lines.Should().Contain($"[yellow]{DateTime.Today:dd/MM/yyyy}[/]: {now}");
    }
}