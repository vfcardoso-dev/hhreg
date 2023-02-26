using System.ComponentModel;
using Spectre.Console.Cli;
using Spectre.Console;
using System.Diagnostics.CodeAnalysis;

namespace hhreg.business;

public sealed class InitCommand : Command<InitCommand.Settings>
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger _logger;

    public InitCommand(ISettingsRepository settingsRepository, ILogger logger) {
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings 
    {
        [Description("Time bank initial balance")]
        [CommandOption("-b|--initial-balance")]
        public string? InitialBalance { get; init; }

        [Description("Workday")]
        [CommandOption("-w|--workday")]
        public string? WorkDay { get; init; }

        [Description("Time input mode: Hours (HH:mm) or Minutes (0..99+)")]
        [CommandOption("-m|--time-input-mode")]
        [DefaultValue(TimeInputMode.Hours)]
        public TimeInputMode TimeInputMode { get; init; }

        [Description("Start calculations at (format: dd/MM/yyyy)")]
        [CommandOption("-s|--start-calculations-at")]
        public string? StartCalculationsAt { get; init; }

        public override ValidationResult Validate()
        {
            if (InitialBalance == null) return ValidationResult.Error("You should inform initial balance");
            if (WorkDay == null) return ValidationResult.Error("You should inform workday.");
            if (StartCalculationsAt == null) return ValidationResult.Error("You should inform your a date to start balance calculations.");
            
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
        if (_settingsRepository.IsAlreadyInitialized()) {
            throw new HhregException("Settings already initialized. You can change it with 'config edit'");
        }
        
        var initialBalance = GetTimeInputValue(settings.TimeInputMode, settings.InitialBalance!);
        var workDay = GetTimeInputValue(settings.TimeInputMode, settings.WorkDay!);
        var startCalculationsAt = DateOnly.Parse(settings.StartCalculationsAt!).ToString("yyyy-MM-dd");

        _settingsRepository.Create(initialBalance, workDay, startCalculationsAt);
        
        _logger.WriteLine($@"Settings [green]SUCCESSFULLY[/] initialized!");
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