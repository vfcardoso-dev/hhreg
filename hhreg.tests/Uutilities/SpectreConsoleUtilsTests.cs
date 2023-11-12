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
    [TestCase(480, 500, "+00:20", null)]
    [TestCase(480, 400, "-01:20", null)]
    [TestCase(480, 480, "00:00", "This is a justification")]
    public void deve_calcular_corretamente_linha_de_summary_de_dayEntry(double workDay, double dayEntryTotalMinutes, string expectedBalance, string justification)
    {
        // Given
        var dayEntry = Fixture.Create<DayEntry>();
        dayEntry.TotalMinutes = dayEntryTotalMinutes;
        dayEntry.Justification = justification;

        // When
        var result = SpectreConsoleUtils.GetDayEntrySummaryRow(dayEntry, workDay);

        // Then
        var totalMinutesTs = TimeSpan.FromMinutes(dayEntry.TotalMinutes);
        
        ExtractText(result[0]).Should().Be(dayEntry.Day!.ToDateOnly().ToString());
        ExtractText(result[1]).Should().Be(dayEntry.DayType.ToString());
        ExtractText(result[2]).Should().Be(string.Join(" / ", dayEntry.TimeEntries.Select(x => x.Time)));
        ExtractText(result[3]).Should().Be(totalMinutesTs.ToTimeString());
        ExtractText(result[4]).Should().Be(expectedBalance);
        ExtractText(result[5]).Should().Be(dayEntry.Justification ?? "-");
    }
    
    [TestCase(480, 500, 20, "+00:20", "+00:40", null)]
    [TestCase(480, 400, -20, "-01:20", "-01:40", null)]
    [TestCase(480, 480, 0, "00:00", "00:00", "This is a justification")]
    public void deve_calcular_corretamente_linha_de_balance_de_dayEntry(
        double workDay, double dayEntryTotalMinutes, double initialAcc, string expectedBalance, string expectedAcc, string justification)
    {
        // Given
        var dayEntry = Fixture.Create<DayEntry>();
        dayEntry.TotalMinutes = dayEntryTotalMinutes;
        dayEntry.Justification = justification;
        var accumulated = initialAcc;

        // When
        var result = SpectreConsoleUtils.GetDayEntryBalanceRow(dayEntry, workDay, ref accumulated);

        // Then
        var workDayTs = TimeSpan.FromMinutes(workDay);
        var totalMinutesTs = TimeSpan.FromMinutes(dayEntry.TotalMinutes);
        var balance = totalMinutesTs.Subtract(workDayTs);

        var timeEntries = dayEntry.TimeEntries.Any() 
            ? string.Join(" / ", dayEntry.TimeEntries.Select(x => x.Time)) 
            : dayEntry.Justification!;
        
        ExtractText(result[0]).Should().Be(dayEntry.Day!.ToDateOnly().ToString());
        ExtractText(result[1]).Should().Be(dayEntry.DayType.ToString());
        ExtractText(result[2]).Should().Be(timeEntries);
        ExtractText(result[3]).Should().Be(totalMinutesTs.ToTimeString());
        ExtractText(result[4]).Should().Be(expectedBalance);
        ExtractText(result[5]).Should().Be(expectedAcc);
        accumulated.Should().Be(initialAcc + balance.TotalMinutes);
    }

    private static string ExtractText(IRenderable renderable)
    {
        return renderable.GetSegments(AnsiConsole.Console).First().Text;
    }
}