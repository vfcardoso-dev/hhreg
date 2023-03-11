using hhreg.business;
using NSubstitute;
using FluentAssertions;
using AutoFixture;
using hhreg.business.domain;

namespace hhreg.tests;

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
        var today = DateTime.Today.ToString("yyyy-MM-dd");
        var now = TimeOnly.FromDateTime(DateTime.Now).ToTimeString();
        var dayEntry = Fixture.Create<DayEntry>();
        AddSingleton<ITimeRepository>(_timeRepository!);
        AddSingleton<ILogger>(Logger);

        dayEntry.Day = today;

        _timeRepository!.GetOrCreateDay(Arg.Any<string>()).Returns(dayEntry);

        var app = CreateCommandApp((config) => config.AddCommand<EntryNowCommand>("now"));

        // When
        var output = app.Run(new []{"now"});
        
        // Then
        output.Should().Be(0);
        _timeRepository!.Received().GetOrCreateDay(dayEntry.Day);
        _timeRepository!.Received().CreateTime(dayEntry.Id, Arg.Is<string>(x => x == now));
        Logger.MethodHits.Where(x => x == "WriteLine").Should().HaveCount(2);
        Logger.Lines.Should().Contain("Day entry [green]SUCCESSFULLY[/] created!");
        Logger.Lines.Should().Contain($"[yellow]{DateTime.Today:dd/MM/yyyy}[/]: {now}");
    }
}