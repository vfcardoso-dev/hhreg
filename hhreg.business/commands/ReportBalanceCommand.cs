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
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger _logger;

    public ReportBalanceCommand(
        ITimeRepository timeRepository, 
        ISettingsRepository settingsRepository,
        ILogger logger) : base(timeRepository, logger) 
    {
        _timeRepository = timeRepository;
        _settingsRepository = settingsRepository;
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
                return ValidationResult.Error("Tail must have a positive value.");
            }
            
            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        CheckInvalidTimeEntries();

        var cfg = _settingsRepository.Get()!;
        var offsetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(settings.Tail * -1));
        var startCalculationsAt = DateOnly.Parse(cfg.StartCalculationsAt);

        if (startCalculationsAt > offsetDate) {
            throw new HhregException($"Configuration is set to start balance calculations after the offset date. StartCalculationsAt: {startCalculationsAt}; OffsetDate: {offsetDate}");
        }
        
        var offsetAccumulatedBalance = _timeRepository.GetAccumulatedBalance(cfg, offsetDate.AddDays(-1).ToString("yyyy-MM-dd"));
        var dayEntries = _timeRepository.GetDayEntries(offsetDate.ToString("yyyy-MM-dd"), DateTime.Today.ToString("yyyy-MM-dd"));

        var rows = new List<Text[]>();
        
        foreach(var dayEntry in dayEntries)
        {
            rows.Add(SpectreConsoleUtils.GetDayEntryBalanceRow(dayEntry, cfg.WorkDay, ref offsetAccumulatedBalance));
        }

        _logger.WriteTable(SpectreConsoleUtils.GetDayEntryBalanceHeaders(), rows);
        return 0;
    }
}