using FluentAssertions;
using Hhreg.Business.Commands;
using Hhreg.Business.Domain;
using Hhreg.Business.Infrastructure;
using Hhreg.Business.Repositories;
using Hhreg.Business.Utilities;
using Hhreg.Tests.Infrastructure;
using NSubstitute;

namespace Hhreg.Tests.Commands;

public class ReportBalanceCommandTests : UnitTestsBase
{
    private const double WorkDayInMinutes = 480;
    private const int AcumuladoColumn = 5;

    private ITimeRepository? _timeRepository;
    private ISettingsService? _settingsService;

    [SetUp]
    public void ReportBalanceCommandTests_SetUp()
    {
        _timeRepository = Substitute.For<ITimeRepository>();
        _settingsService = Substitute.For<ISettingsService>();
        _timeRepository.GetInvalidDayEntries().Returns(Array.Empty<DayEntry>());
    }

    [Test]
    public void deve_acumular_sem_aplicar_bonus_quando_iteracao_fica_no_mes_corrente()
    {
        // Given
        var today = DateTime.Today.ToDateOnly();
        var entries = new[]
        {
            BuildDayEntry(today.AddDays(-2), totalMinutes: 540),
            BuildDayEntry(today.AddDays(-1), totalMinutes: 540),
            BuildDayEntry(today, totalMinutes: 540)
        };
        ConfigureMocks(
            settings: BuildSettings(bonusPercentage: 50),
            baselineBalance: 1000,
            dayEntries: entries,
            monthDeltas: new Dictionary<string, double>
            {
                { today.ToString("yyyy-MM"), 180 }
            });

        var app = CreateCommandApp(c => c.AddCommand<ReportBalanceCommand>("balance"));

        // When
        var output = app.Run(new[] { "balance", "--tail", "5" });

        // Then
        output.Should().Be(0);
        // Final acumulado = baseline (1000) + 3 dias de +60 cada = 1180
        Logger.Rows.Last()[AcumuladoColumn].Should().Be(FormatAccumulated(1180));
    }

    [Test]
    public void deve_aplicar_bonus_ao_cruzar_mes_quando_delta_mensal_foi_positivo()
    {
        // Given
        var jan30 = new DateOnly(2025, 1, 30);
        var entries = new[]
        {
            BuildDayEntry(jan30, totalMinutes: 600),                 // delta +120
            BuildDayEntry(jan30.AddDays(1), totalMinutes: 600),      // delta +120 (Jan 31)
            BuildDayEntry(jan30.AddDays(2), totalMinutes: 540),      // delta +60  (Feb 1) — gatilho do bônus
            BuildDayEntry(jan30.AddDays(3), totalMinutes: 540)       // delta +60  (Feb 2)
        };
        ConfigureMocks(
            settings: BuildSettings(bonusPercentage: 50),
            baselineBalance: 0,
            dayEntries: entries,
            monthDeltas: new Dictionary<string, double>
            {
                { "2025-01", 240 }, // delta total de janeiro: positivo
                { "2025-02", 120 }
            });

        var app = CreateCommandApp(c => c.AddCommand<ReportBalanceCommand>("balance"));

        // When
        var output = app.Run(new[] { "balance", "--tail", "5" });

        // Then
        output.Should().Be(0);
        Logger.Rows.Should().HaveCount(4);
        Logger.Rows[0][AcumuladoColumn].Should().Be(FormatAccumulated(120));   // Jan 30
        Logger.Rows[1][AcumuladoColumn].Should().Be(FormatAccumulated(240));   // Jan 31
        // Jan -> Feb: aplica 50% sobre 240 = 120. acc = 240 + 120 + 60 = 420
        Logger.Rows[2][AcumuladoColumn].Should().Be(FormatAccumulated(420));   // Feb 1
        Logger.Rows[3][AcumuladoColumn].Should().Be(FormatAccumulated(480));   // Feb 2
    }

    [Test]
    public void nao_deve_aplicar_bonus_ao_cruzar_mes_quando_delta_mensal_foi_negativo()
    {
        // Given
        var jan31 = new DateOnly(2025, 1, 31);
        var entries = new[]
        {
            BuildDayEntry(jan31, totalMinutes: 420),                 // delta -60 (Jan 31)
            BuildDayEntry(jan31.AddDays(1), totalMinutes: 540)       // delta +60 (Feb 1)
        };
        ConfigureMocks(
            settings: BuildSettings(bonusPercentage: 50),
            baselineBalance: 0,
            dayEntries: entries,
            monthDeltas: new Dictionary<string, double>
            {
                { "2025-01", -120 }, // janeiro negativo: sem bônus
                { "2025-02", 60 }
            });

        var app = CreateCommandApp(c => c.AddCommand<ReportBalanceCommand>("balance"));

        // When
        var output = app.Run(new[] { "balance", "--tail", "5" });

        // Then
        output.Should().Be(0);
        Logger.Rows[0][AcumuladoColumn].Should().Be(FormatAccumulated(-60));   // Jan 31
        // Jan -> Feb: sem bônus, acc continua. -60 + 60 = 0
        Logger.Rows[1][AcumuladoColumn].Should().Be(FormatAccumulated(0));     // Feb 1
    }

    [Test]
    public void nao_deve_aplicar_bonus_quando_percentual_e_zero_mesmo_cruzando_mes_positivo()
    {
        // Given
        var jan31 = new DateOnly(2025, 1, 31);
        var entries = new[]
        {
            BuildDayEntry(jan31, totalMinutes: 600),                 // delta +120
            BuildDayEntry(jan31.AddDays(1), totalMinutes: 540)       // delta +60
        };
        ConfigureMocks(
            settings: BuildSettings(bonusPercentage: 0),
            baselineBalance: 0,
            dayEntries: entries,
            monthDeltas: new Dictionary<string, double>
            {
                { "2025-01", 120 },
                { "2025-02", 60 }
            });

        var app = CreateCommandApp(c => c.AddCommand<ReportBalanceCommand>("balance"));

        // When
        var output = app.Run(new[] { "balance", "--tail", "5" });

        // Then
        output.Should().Be(0);
        Logger.Rows[0][AcumuladoColumn].Should().Be(FormatAccumulated(120));
        // Sem bonus apesar do delta positivo de janeiro
        Logger.Rows[1][AcumuladoColumn].Should().Be(FormatAccumulated(180));
    }

    private void ConfigureMocks(
        Settings settings,
        double baselineBalance,
        IEnumerable<DayEntry> dayEntries,
        IDictionary<string, double> monthDeltas)
    {
        AddSingleton<ITimeRepository>(_timeRepository!);
        AddSingleton<ISettingsService>(_settingsService!);
        AddSingleton<ILogger>(Logger);

        _settingsService!.GetSettings().Returns(settings);
        _timeRepository!.GetAccumulatedBalance(Arg.Any<DateOnly>()).Returns(baselineBalance);
        _timeRepository.GetMonthlyDeltas(Arg.Any<DateOnly>()).Returns(monthDeltas);
        _timeRepository.GetDayEntries(Arg.Any<DateOnly>(), Arg.Any<DateOnly>()).Returns(dayEntries);
    }

    private static Settings BuildSettings(double bonusPercentage)
    {
        return new Settings
        {
            StartBalanceInMinutes = 0,
            WorkDayInMinutes = WorkDayInMinutes,
            EntryToleranceInMinutes = 0,
            LastBalanceCutoff = "2020-01-01",
            MonthlyBonusPercentage = bonusPercentage
        };
    }

    private static DayEntry BuildDayEntry(DateOnly day, double totalMinutes)
    {
        return new DayEntry
        {
            Id = day.DayNumber,
            Day = day.ToString("yyyy-MM-dd"),
            DayType = DayType.Work,
            TotalMinutes = totalMinutes,
            TimeEntries = new[]
            {
                new TimeEntry { Time = "08:00" },
                new TimeEntry { Time = "17:00" }
            }
        };
    }

    private static string FormatAccumulated(double minutes)
    {
        var ts = TimeSpan.FromMinutes(minutes);
        var signal = ts > TimeSpan.Zero ? "+" : "";
        return $"{signal}{ts.ToTimeString()}";
    }
}
