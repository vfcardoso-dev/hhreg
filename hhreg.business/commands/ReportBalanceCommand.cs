using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace hhreg.business;

public sealed class ReportBalanceCommand : ReportCommandBase<ReportBalanceCommand.Settings>
{
    private readonly ITimeRepository _timeRepository;
    private readonly ISettingsRepository _settingsRepository;

    public ReportBalanceCommand(
        ITimeRepository timeRepository, 
        ISettingsRepository settingsRepository) : base(timeRepository) 
    {
        _timeRepository = timeRepository;
        _settingsRepository = settingsRepository;
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

        var table = new Table();
        table.AddColumns(SpectreConsoleUtils.GetDayEntryBalanceHeaders());
        
        foreach(var dayEntry in dayEntries)
        {
            table.AddRow(SpectreConsoleUtils.GetDayEntryBalanceRow(dayEntry, cfg.WorkDay, ref offsetAccumulatedBalance));
        }
        
        AnsiConsole.Write(table);
        return 0;
    }
}