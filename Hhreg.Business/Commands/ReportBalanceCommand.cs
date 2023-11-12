using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using hhreg.business.exceptions;
using hhreg.business.infrastructure;
using hhreg.business.repositories;
using hhreg.business.utilities;
using Spectre.Console;
using Spectre.Console.Cli;

namespace hhreg.business.commands;

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

    public sealed class Settings : CommandSettings {
        [Description("Retrieve last N days")]
        [CommandOption("-t|--tail")]
        [DefaultValue(5)]
        public int Tail { get; init; }

        public override ValidationResult Validate()
        {
            if (Tail < 1) {
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

        if (startCalculationsAt > offsetDate) {
            throw new HhregException(string.Format(HhregMessages.ConfigurationIsSetToStartBalanceCalculationsAfterTheOffsetDate, startCalculationsAt, offsetDate));
        }
        
        var offsetAccumulatedBalance = _timeRepository.GetAccumulatedBalance(cfg, offsetDate.AddDays(-1));
        var dayEntries = _timeRepository.GetDayEntries(offsetDate, DateTime.Today.ToDateOnly());

        var rows = new List<Text[]>();
        
        foreach(var dayEntry in dayEntries)
        {
            rows.Add(SpectreConsoleUtils.GetDayEntryBalanceRow(dayEntry, cfg.WorkDayInMinutes, ref offsetAccumulatedBalance));
        }

        _logger.WriteTable(SpectreConsoleUtils.GetDayEntryBalanceHeaders(), rows);
        return 0;
    }
}