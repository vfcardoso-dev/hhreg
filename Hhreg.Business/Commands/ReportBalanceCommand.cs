using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Hhreg.Business.Exceptions;
using Hhreg.Business.Infrastructure;
using Hhreg.Business.Repositories;
using Hhreg.Business.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Hhreg.Business.Commands;

public sealed class ReportBalanceCommand : ReportCommandBase<ReportBalanceCommand.Settings>
{
    private readonly ITimeRepository _timeRepository;
    private readonly ISettingsService _settingsService;
    private readonly ILogger _logger;

    public ReportBalanceCommand(
        ITimeRepository timeRepository,
        ISettingsService settingsService,
        ILogger logger) : base(timeRepository, logger)
    {
        _timeRepository = timeRepository;
        _settingsService = settingsService;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Recupera Últimos N dias")]
        [CommandOption("-t|--tail")]
        [DefaultValue(5)]
        public int Tail { get; init; }

        public override ValidationResult Validate()
        {
            if (Tail < 1)
            {
                return ValidationResult.Error(HhregMessages.TailMustHaveAPositiveValue);
            }

            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        CheckInvalidTimeEntries();

        var cfg = _settingsService.GetSettings();
        var offsetDate = DateTime.Today.AddDays((settings.Tail + 1) * -1).ToDateOnly();
        var startCalculationsAt = cfg.LastBalanceCutoff.ToDateOnly();

        if (startCalculationsAt > offsetDate)
        {
            throw new HhregException(string.Format(HhregMessages.ConfigurationIsSetToStartBalanceCalculationsAfterTheOffsetDate, startCalculationsAt, offsetDate));
        }

        var today = DateTime.Today.ToDateOnly();
        var offsetAccumulatedBalance = _timeRepository.GetAccumulatedBalance(offsetDate.AddDays(-1));
        var monthDeltas = _timeRepository.GetMonthlyDeltas(today);
        var dayEntries = _timeRepository.GetDayEntries(offsetDate, today);

        var rows = new List<Text[]>();
        string? currentYearMonth = null;

        foreach (var dayEntry in dayEntries)
        {
            var entryYearMonth = dayEntry.Day!.Substring(0, 7);
            if (currentYearMonth != null && currentYearMonth != entryYearMonth)
            {
                var closedMonthDelta = monthDeltas.TryGetValue(currentYearMonth, out var d) ? d : 0;
                offsetAccumulatedBalance += Calculations.ComputeMonthBonus(closedMonthDelta, monthClosed: true, cfg.MonthlyBonusPercentage);
            }
            currentYearMonth = entryYearMonth;

            rows.Add(SpectreConsoleUtils.GetDayEntryBalanceRow(dayEntry, cfg.WorkDayInMinutes, ref offsetAccumulatedBalance));
        }

        _logger.WriteTable(SpectreConsoleUtils.GetDayEntryBalanceHeaders(), rows);
        return 0;
    }
}