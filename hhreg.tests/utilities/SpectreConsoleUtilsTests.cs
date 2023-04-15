using AutoFixture;
using FluentAssertions;
using hhreg.business.domain;
using hhreg.business.utilities;
using hhreg.tests.infrastructure;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace hhreg.tests.utilities;

public class SpectreConsoleUtilsTests : UnitTestsBase
{
    [Test]
    public void deve_calcular_corretamente_linha_de_summary_de_dayEntry()
    {
        // Given
        var dayEntry = Fixture.Create<DayEntry>();
        var workDay = Fixture.Create<double>();

        // When
        var result = SpectreConsoleUtils.GetDayEntrySummaryRow(dayEntry, workDay);

        // Then
        var workDayTs = TimeSpan.FromMinutes(workDay);
        var totalMinutesTs = TimeSpan.FromMinutes(dayEntry.TotalMinutes);
        var balance = totalMinutesTs.Subtract(workDayTs);
        var balanceSignal = balance > TimeSpan.Zero ? "+" : "";
        
        ExtractText(result[0]).Should().Be(dayEntry.Day!.ToDateOnly().ToString());
        ExtractText(result[1]).Should().Be(dayEntry.DayType.ToString());
        ExtractText(result[2]).Should().Be(string.Join(" / ", dayEntry.TimeEntries.Select(x => x.Time)));
        ExtractText(result[3]).Should().Be(totalMinutesTs.ToTimeString());
        ExtractText(result[4]).Should().Be($"{balanceSignal}{balance.ToTimeString()}");
        ExtractText(result[5]).Should().Be(dayEntry.Justification ?? "-");
    }
    
    [Test]
    public void deve_calcular_corretamente_linha_de_balance_de_dayEntry()
    {
        // Given
        var dayEntry = Fixture.Create<DayEntry>();
        var workDay = Fixture.Create<double>();
        var startingAccumulation = Fixture.Create<double>();
        var accumulated = startingAccumulation;

        // When
        var result = SpectreConsoleUtils.GetDayEntryBalanceRow(dayEntry, workDay, ref accumulated);

        // Then
        var workDayTs = TimeSpan.FromMinutes(workDay);
        var totalMinutesTs = TimeSpan.FromMinutes(dayEntry.TotalMinutes);
        var balance = totalMinutesTs.Subtract(workDayTs);
        var balanceSignal = balance > TimeSpan.Zero ? "+" : "";

        var accumulatedTs = TimeSpan.FromMinutes(accumulated);
        var accumulatedSignal = accumulatedTs > TimeSpan.Zero ? "+" : "";
        var timeEntries = dayEntry.TimeEntries.Any() 
            ? string.Join(" / ", dayEntry.TimeEntries.Select(x => x.Time)) 
            : dayEntry.Justification!;
        
        ExtractText(result[0]).Should().Be(dayEntry.Day!.ToDateOnly().ToString());
        ExtractText(result[1]).Should().Be(dayEntry.DayType.ToString());
        ExtractText(result[2]).Should().Be(timeEntries);
        ExtractText(result[3]).Should().Be(totalMinutesTs.ToTimeString());
        ExtractText(result[4]).Should().Be($"{balanceSignal}{balance.ToTimeString()}");
        ExtractText(result[5]).Should().Be($"{accumulatedSignal}{accumulatedTs.ToTimeString()}");
        accumulated.Should().Be(startingAccumulation + balance.TotalMinutes);
    }

    private static string ExtractText(IRenderable renderable)
    {
        return renderable.GetSegments(AnsiConsole.Console).First().Text;
    }
}