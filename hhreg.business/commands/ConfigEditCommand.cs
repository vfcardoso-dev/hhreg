using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace hhreg.business;

public sealed class ConfigEditCommand : Command<ConfigEditCommand.Settings>
{
    private readonly ISettingsRepository _settingsRepository;

    public ConfigEditCommand(ISettingsRepository settingsRepository) {
        _settingsRepository = settingsRepository;
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Time bank initial balance (in minutes)")]
        [CommandOption("--initial-balance-in-minutes")]
        public double? InitialBalanceMinutes { get; init; }

        [Description("Workday (in minutes)")]
        [CommandOption("--workday-in-minutes")]
        public double? WorkDayMinutes { get; init; }

        [Description("Time bank initial balance (in hours)")]
        [CommandOption("-b|--initial-balance-in-hours")]
        public string? InitialBalanceHours { get; init; }

        [Description("Workday (in hours)")]
        [CommandOption("-w|--workday-in-hours")]
        public string? WorkDayHours { get; init; }

        public override ValidationResult Validate()
        {
            if (InitialBalanceMinutes == null && InitialBalanceHours == null) {
                return ValidationResult.Error("You should inform an initial balance for the hour bank (in minutes or hours).");
            }
                
            if (WorkDayMinutes == null && WorkDayHours == null) {
                return ValidationResult.Error("You should inform your workday (in minutes or hours).");
            }
            
            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var initialBalance = settings.InitialBalanceMinutes ?? ExtractMinutes(settings.InitialBalanceHours!);
        var workDay = settings.WorkDayMinutes ?? ExtractMinutes(settings.WorkDayHours!);

        _settingsRepository.Update(initialBalance, workDay);
        
        AnsiConsole.MarkupLineInterpolated($@"Settings [green]SUCCESSFULLY[/] updated!");
        return 0;
    }

    private double ExtractMinutes(string time) {
        return TimeSpan.Parse(time).TotalMinutes;
    }
}

