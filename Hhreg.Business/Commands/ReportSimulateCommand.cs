using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Hhreg.Business.Domain;
using Hhreg.Business.Exceptions;
using Hhreg.Business.Infrastructure;
using Hhreg.Business.Repositories;
using Hhreg.Business.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Hhreg.Business.Commands;

public sealed class ReportSimulateCommand : ReportCommandBase<ReportSimulateCommand.Settings>
{
    private readonly ITimeRepository _timeRepository;
    private readonly ISettingsService _settingsService;
    private readonly ILogger _logger;

    public ReportSimulateCommand(
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
        [Description("Define a data da simulação. (formato: dd/MM/yyyy)")]
        [CommandOption("-d|--day")]
        public string? Day { get; init; }

        [Description("Marcações a simular (formato: HH:mm)")]
        [CommandArgument(0, "[entries]")]
        public string[] Entries { get; init; } = Array.Empty<string>();

        public override ValidationResult Validate()
        {
            if (Day != null && !DateOnly.TryParse(Day, out var _))
            {
                return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidDateFormat, Day));
            }

            foreach (var entry in Entries)
            {
                if (TimeSpan.TryParse(entry, out var time))
                {
                    if (time < TimeSpan.Zero)
                    {
                        return ValidationResult.Error(HhregMessages.EntryTimesMustBePositive);
                    }
                }
                else
                {
                    return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidTimeFormat, entry));
                }
            }

            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var cfg = _settingsService.GetSettings();
        var inputDay = settings.Day == null ? DateTime.Today.ToDateOnly() : settings.Day!.ToDateOnly();
        var offsetDate = DateTime.Today.AddDays(-1).ToDateOnly();
        var startCalculationsAt = cfg.LastBalanceCutoff.ToDateOnly();
        var simulatedEntries = settings.Entries.Select(x => new TimeEntry { Time = x });

        if (startCalculationsAt > offsetDate)
        {
            throw new HhregException(string.Format(HhregMessages.ConfigurationIsSetToStartBalanceCalculationsAfterTheOffsetDate, startCalculationsAt, offsetDate));
        }

        var offsetAccumulatedBalance = _timeRepository.GetAccumulatedBalance(offsetDate);
        var dayEntry = _timeRepository.GetDayEntries(inputDay, inputDay)?.FirstOrDefault();

        if (dayEntry == null)
        {
            throw new HhregException(HhregMessages.InformedDayIsNotRegistered);
        }

        dayEntry.TimeEntries = dayEntry.TimeEntries.Union(simulatedEntries);
        dayEntry.TotalMinutes = Calculations.GetTotalMinutes(
            dayEntry.TimeEntries.Select(x => x.Time!),
            dayEntry.DayType,
            cfg.EntryToleranceInMinutes,
            cfg.WorkDayInMinutes);

        var rows = new[] {SpectreConsoleUtils.GetDayEntryBalanceRow(dayEntry, cfg.WorkDayInMinutes, ref offsetAccumulatedBalance)}.ToList();

        _logger.WriteTable(SpectreConsoleUtils.GetDayEntryBalanceHeaders(), rows);
        return 0;
    }
}