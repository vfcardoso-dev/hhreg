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
        [Description("Time bank initial balance")]
        [CommandOption("-b|--initial-balance")]
        public string? InitialBalance { get; init; }

        [Description("Workday")]
        [CommandOption("-w|--workday")]
        public string? WorkDay { get; init; }

        [Description("Time input mode (minutes or HH:mm)")]
        [CommandOption("-f|--time-input-mode")]
        [DefaultValue(TimeInputMode.Hours)]
        public TimeInputMode TimeInputMode { get; init; }

        [Description("Start calculations at (format: dd/MM/yyyy)")]
        [CommandOption("-s|--start-calculations-at")]
        public string? StartCalculationsAt { get; init; }

        public override ValidationResult Validate()
        {
            if (InitialBalance == null) return ValidationResult.Error("You should inform initial balance");
            if (WorkDay == null) return ValidationResult.Error("You should inform workday.");
            
            if (TimeInputMode == TimeInputMode.Hours && !TimeSpan.TryParse(InitialBalance, out var _))
                return ValidationResult.Error($"Could not parse '{InitialBalance}' as a valid time format.");
            
            if (TimeInputMode == TimeInputMode.Minutes && !int.TryParse(InitialBalance, out var _))
                return ValidationResult.Error($"Could not parse '{InitialBalance}' as a valid integer format.");
            
            if (TimeInputMode == TimeInputMode.Hours && !int.TryParse(WorkDay, out var _))
                return ValidationResult.Error($"Could not parse '{WorkDay}' as a valid time format.");
            
            if (TimeInputMode == TimeInputMode.Minutes && !int.TryParse(WorkDay, out var _))
                return ValidationResult.Error($"Could not parse '{WorkDay}' as a valid integer format.");
            
            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var initialBalance = GetTimeInputValue(settings.TimeInputMode, settings.InitialBalance!);
        var workDay = GetTimeInputValue(settings.TimeInputMode, settings.WorkDay!);
        var startCalculationsAt = settings.StartCalculationsAt != null 
            ? DateOnly.Parse(settings.StartCalculationsAt!).ToString("yyyy-MM-dd") : null;

        _settingsRepository.Update(initialBalance, workDay, startCalculationsAt);
        
        AnsiConsole.MarkupLineInterpolated($@"Settings [green]SUCCESSFULLY[/] updated!");
        return 0;
    }

    private double GetTimeInputValue(TimeInputMode mode, string value)
    {
        return mode switch
        {
            TimeInputMode.Hours => TimeSpan.Parse(value).TotalMinutes,
            TimeInputMode.Minutes => int.Parse(value),
            _ => throw new HhregException($"Invalid input format on value '{value}'")
        };
    }
}

