using AutoFixture;
using FluentAssertions;
using hhreg.business.commands;
using hhreg.business.domain;
using hhreg.business.exceptions;
using hhreg.business.infrastructure;
using hhreg.business.repositories;
using hhreg.business.utilities;
using hhreg.tests.infrastructure;
using hhreg.tests.utilities;
using NSubstitute;

namespace hhreg.tests.commands;

public class EntryOverrideCommandTests : UnitTestsBase
{
    private ITimeRepository? _timeRepository;

    [SetUp]
    public void EntryOverrideCommandTests_SetUp() {
        _timeRepository = Substitute.For<ITimeRepository>();
    }

    [Test]
    public void deve_falhar_ao_nao_informar_data_ou_definir_hoje()
    {
        // Given
        var dayType = DayType.Work;
        var entry = TimeSpan.FromMinutes(Math.Abs(Fixture.Create<int>())).ToTimeString();
        AddSingleton<ITimeRepository>(_timeRepository!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<EntryOverrideCommand>("override"));

        // When
        Action action = () => app.Run(new []{"override", "-y", dayType.ToString(), entry});
        
        // Then
        _timeRepository!.DidNotReceive().CreateTime(Arg.Any<long>(), Arg.Any<string[]>());
        action.Should().Throw<Exception>().WithMessage(HhregMessages.YouShouldInformADayToLog);
    }

    [Test]
    public void deve_falhar_ao_informar_data_invalida()
    {
        // Given
        var invalidDates = new string[]{"01/13/2023", "32/03/2023", "10/03/0000"};
        var entry = TimeSpan.FromMinutes(Math.Abs(Fixture.Create<int>())).ToTimeString();
        AddSingleton<ITimeRepository>(_timeRepository!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<EntryOverrideCommand>("override"));

        // When
        var entryDate = invalidDates[Fixture.CreateBetween(0, 2)];
        Action action = () => app.Run(new []{"override", "-d", entryDate, entry});
        
        // Then
        _timeRepository!.DidNotReceive().CreateTime(Arg.Any<long>(), Arg.Any<string[]>());
        action.Should().Throw<Exception>().WithMessage(string.Format(HhregMessages.CouldNotParseAsAValidDateFormat, entryDate));
    }

    [Test]
    public void deve_falhar_ao_nao_informar_ao_menos_uma_entrada_ou_justificativa()
    {
        // Given
        AddSingleton<ITimeRepository>(_timeRepository!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<EntryOverrideCommand>("override"));

        // When
        Action action = () => app.Run(new []{"override", "-t"});
        
        // Then
        _timeRepository!.DidNotReceive().CreateTime(Arg.Any<long>(), Arg.Any<string[]>());
        action.Should().Throw<Exception>().WithMessage(HhregMessages.YouShouldInformAtLeastOneTimeEntryOrSetAJustificative);
    }

    [Test]
    public void deve_falhar_ao_informar_entradas_nao_positivas()
    {
        // Given
        var badEntry = TimeSpan.FromMinutes(Math.Abs(Fixture.Create<int>()) * -1).ToTimeString();
        AddSingleton<ITimeRepository>(_timeRepository!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<EntryOverrideCommand>("override"));

        // When
        Action action = () => app.Run(new []{"override", "-t", badEntry});
        
        // Then
        _timeRepository!.DidNotReceive().CreateTime(Arg.Any<long>(), Arg.Any<string[]>());
        action.Should().Throw<Exception>().WithMessage(HhregMessages.EntryTimesMustBePositive);
    }

    [Test]
    public void deve_falhar_ao_informar_entradas_invalidas()
    {
        // Given
        var badEntries = new[]{"08:95","24:32","26:02","02:1"};
        AddSingleton<ITimeRepository>(_timeRepository!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<EntryOverrideCommand>("override"));

        // When
        var badEntry = badEntries[Fixture.CreateBetween(0, 3)];
        Action action = () => app.Run(new []{"override", "-t", badEntry});
        
        // Then
        _timeRepository!.DidNotReceive().CreateTime(Arg.Any<long>(), Arg.Any<string[]>());
        action.Should().Throw<Exception>().WithMessage(string.Format(HhregMessages.CouldNotParseAsAValidTimeFormat, badEntry));
    }

    [Test]
    public void deve_falhar_ao_tentar_sobrescrever_entradas_de_uma_data_nao_criada()
    {
        // Given
        AddSingleton<ITimeRepository>(_timeRepository!);
        AddSingleton<ILogger>(Logger);

        var app = CreateCommandApp((config) => config.AddCommand<EntryOverrideCommand>("override"));

        // When
        Action action = () => app.Run(new []{"override", "-t", "08:00"});
        
        // Then
        _timeRepository!.DidNotReceive().CreateTime(Arg.Any<long>(), Arg.Any<string[]>());
        action.Should().Throw<Exception>().WithMessage(string.Format(HhregMessages.CannotOverrideANotYetCreatedDay, DateTime.Today.ToString("dd/MM/yyyy")));
    }

    [Test]
    public void deve_ser_capaz_de_lancar_entradas()
    {
        // Given
        var entry1 = TimeSpan.FromMinutes(Math.Abs(Fixture.Create<int>())).ToTimeString();
        var entry2 = TimeSpan.FromMinutes(Math.Abs(Fixture.Create<int>())).ToTimeString();
        var entry3 = TimeSpan.FromMinutes(Math.Abs(Fixture.Create<int>())).ToTimeString();
        var entry4 = TimeSpan.FromMinutes(Math.Abs(Fixture.Create<int>())).ToTimeString();
        var dayEntry = Fixture.Create<DayEntry>();
        AddSingleton<ITimeRepository>(_timeRepository!);
        AddSingleton<ILogger>(Logger);

        _timeRepository!.GetDayEntry(Arg.Any<string>()).Returns(dayEntry);

        var app = CreateCommandApp((config) => config.AddCommand<EntryOverrideCommand>("override"));

        // When
        var output = app.Run(new []{"override", "-t", entry1, entry2, entry3, entry4});
        
        // Then
        output.Should().Be(0);
        _timeRepository!.Received().OverrideDayEntry(dayEntry.Id, Arg.Any<string>(), Arg.Any<DayType>(),
            Arg.Is<string[]>(x => x.Contains(entry1) && x.Contains(entry2) && x.Contains(entry3) && x.Contains(entry4)));
        Logger.MethodHits.Where(x => x == "WriteLine").Should().HaveCount(2);
        Logger.Lines.Should().Contain("Day entry [green]SUCCESSFULLY[/] overridden!");
        Logger.Lines.Should().Contain($"[yellow]{DateTime.Today:dd/MM/yyyy}[/]: {entry1} / {entry2} / {entry3} / {entry4}");
    }

    [Test]
    public void deve_ser_capaz_de_lancar_justificativa()
    {
        // Given
        var justificative = Fixture.Create<string>();
        var dayType = Fixture.CreateAnyBut(DayType.Work);
        var dayEntry = Fixture.Create<DayEntry>();
        AddSingleton<ITimeRepository>(_timeRepository!);
        AddSingleton<ILogger>(Logger);

        _timeRepository!.GetDayEntry(Arg.Any<string>()).Returns(dayEntry);

        var app = CreateCommandApp((config) => config.AddCommand<EntryOverrideCommand>("override"));

        // When
        var output = app.Run(new []{"override", "-t", "-y", dayType.ToString(), "-j", justificative});
        
        // Then
        output.Should().Be(0);
        _timeRepository!.Received().OverrideDayEntry(dayEntry.Id, justificative, dayType, Arg.Any<string[]>());

        Logger.MethodHits.Where(x => x == "WriteLine").Should().HaveCount(2);
        Logger.Lines.Should().Contain("Day entry [green]SUCCESSFULLY[/] overridden!");
        Logger.Lines.Should().Contain($"[yellow]{DateTime.Today:dd/MM/yyyy}[/]: {justificative}");
    }
}