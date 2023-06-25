using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace hhreg.business;

public sealed class ReportDayCommand : ReportCommandBase<ReportDayCommand.Settings>
{
    private readonly ITimeRepository _timeRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger _logger;

    public ReportDayCommand(
        ITimeRepository timeRepository, 
        ISettingsRepository settingsRepository,
        ILogger logger) : base(timeRepository, logger)
    {
        _timeRepository = timeRepository;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings {
        [Description("Show entries for a given day")]
        [CommandArgument(0, "<day>")]
        public string? Day { get; init; }

        public override ValidationResult Validate()
        {
            if (Day == null) {
                return ValidationResult.Error(HhregMessages.YouShouldInformADay);
            }

            if (!DateOnly.TryParse(Day, out var _)) {
                return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidDateFormat, Day));
            }
            
            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        CheckInvalidTimeEntries();

        var cfg = _settingsRepository.Get()!;

        var day = DateOnly.Parse(settings.Day!);
        var dayEntry = _timeRepository.GetDayEntry(day.ToString("yyyy-MM-dd"));

        if (dayEntry == null) 
        {
            throw new HhregException(HhregMessages.InformedDayIsNotRegistered);
        }

        _logger.WriteTable(SpectreConsoleUtils.GetDayEntrySummaryHeaders(), 
            new List<Text[]>{{SpectreConsoleUtils.GetDayEntrySummaryRow(dayEntry, cfg.WorkDay)}});
        return 0;
    }
}