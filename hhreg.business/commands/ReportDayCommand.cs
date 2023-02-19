using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace hhreg.business;

public sealed class ReportDayCommand : Command<ReportDayCommand.Settings>
{
    private readonly ITimeRepository _timeRepository;
    private readonly ISettingsRepository _settingsRepository;

    public ReportDayCommand(ITimeRepository timeRepository, ISettingsRepository settingsRepository) {
        _timeRepository = timeRepository;
        _settingsRepository = settingsRepository;
    }

    public sealed class Settings : CommandSettings {
        [Description("Show entries for a given day")]
        [CommandArgument(0, "<day>")]
        public string? Day { get; init; }

        public override ValidationResult Validate()
        {
            if (Day == null) {
                return ValidationResult.Error("You should inform a day.");
            }

            if (!DateOnly.TryParse(Day, out var _)) {
                return ValidationResult.Error($"Could not parse '{Day}' as a valid date format.");
            }
            
            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var cfg = _settingsRepository.Get()!;

        var day = DateOnly.Parse(settings.Day!);
        var dayEntry = _timeRepository.GetDayEntry(day.ToString("yyyy-MM-dd"))!;

        var table = new Table();
        table.AddColumns(dayEntry.RenderSummaryHeaders());
        table.AddRow(dayEntry.RenderSummaryRow(cfg.WorkDay));
        AnsiConsole.Write(table);
        return 0;
    }
}