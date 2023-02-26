using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace hhreg.business;

public sealed class ReportMonthCommand : ReportCommandBase<ReportMonthCommand.Settings>
{
    private readonly ITimeRepository _timeRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger _logger;

    public ReportMonthCommand(
        ITimeRepository timeRepository, 
        ISettingsRepository settingsRepository,
        ILogger logger) : base(timeRepository, logger) 
    {
        _timeRepository = timeRepository;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings {
        [Description("Show entries for a given month (format: MM/yyyy")]
        [CommandArgument(0, "<month>")]
        public string? Month { get; init; }

        public override ValidationResult Validate()
        {
            if (Month == null) {
                return ValidationResult.Error("You should inform a month (MM/yyyy).");
            }

            if (!DateOnly.TryParse(Month, out var _)) {
                return ValidationResult.Error($"Could not parse '{Month}' as a valid date format.");
            }
            
            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        CheckInvalidTimeEntries();

        var cfg = _settingsRepository.Get()!;

        var inputMonth = DateTime.Parse(settings.Month!);
        var start = new DateTime(inputMonth.Year, inputMonth.Month, 1);
        var end = new DateTime(inputMonth.Year, inputMonth.Month, DateTime.DaysInMonth(inputMonth.Year, inputMonth.Month));

        var dayEntries = _timeRepository.GetDayEntries(start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"))!;

        var rows = new List<Text[]>();
        foreach(var day in dayEntries)
        {
            rows.Add(SpectreConsoleUtils.GetDayEntrySummaryRow(day, cfg.WorkDay));
        }

        _logger.WriteTable(SpectreConsoleUtils.GetDayEntrySummaryHeaders(), rows);
        return 0;
    }
}