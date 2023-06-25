using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using hhreg.business.domain.valueObjects;
using hhreg.business.exceptions;
using hhreg.business.infrastructure;
using hhreg.business.repositories;
using hhreg.business.utilities;
using Spectre.Console;
using Spectre.Console.Cli;

namespace hhreg.business.commands;

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

        [Description("Time input mode: Hours (HH:mm) or Minutes (1..999+)")]
        [CommandOption("-m|--time-input-mode")]
        [DefaultValue(TimeInputMode.Hours)]
        public TimeInputMode TimeInputMode { get; init; }

        [Description("Start calculations at (format: dd/MM/yyyy)")]
        [CommandOption("-s|--start-calculations-at")]
        public string? StartCalculationsAt { get; init; }

        public override ValidationResult Validate()
        {
            if (InitialBalance == null) return ValidationResult.Error(HhregMessages.YouShouldInformInitialBalance);
            if (WorkDay == null) return ValidationResult.Error(HhregMessages.YouShouldInformWorkday);
            if (StartCalculationsAt == null) return ValidationResult.Error(HhregMessages.YouShouldInformStartCalculationsAt);
            
            if (TimeInputMode == TimeInputMode.Hours && InitialBalance?.IsTime() == false)
                return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidTimeFormat, InitialBalance));
            
            if (TimeInputMode == TimeInputMode.Minutes && InitialBalance?.IsInteger() == false)
                return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidIntegerFormat, InitialBalance));
            
            if (TimeInputMode == TimeInputMode.Hours && WorkDay?.IsTime() == false)
                return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidTimeFormat, WorkDay));
            
            if (TimeInputMode == TimeInputMode.Minutes && WorkDay?.IsInteger() == false)
                return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidIntegerFormat, WorkDay));

            if (!DateOnly.TryParse(StartCalculationsAt, out var _))
                return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidDateFormat, StartCalculationsAt));
            
            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        if (_settingsRepository.IsAlreadyInitialized()) {
            throw new HhregException(HhregMessages.SettingsAlreadyInitialized);
        }
        
        var initialBalance = GetTimeInputValue(settings.TimeInputMode, settings.InitialBalance!);
        var workDay = GetTimeInputValue(settings.TimeInputMode, settings.WorkDay!);
        var startCalculationsAt = DateOnly.Parse(settings.StartCalculationsAt!).ToString("yyyy-MM-dd");

        _settingsRepository.Create(initialBalance, workDay, startCalculationsAt);
        
        _logger.WriteLine($@"Settings [green]SUCCESSFULLY[/] initialized!");
        return 0;
    }

    private static double GetTimeInputValue(TimeInputMode mode, string value)
    {
        return mode switch
        {
            TimeInputMode.Hours => TimeSpan.Parse(value).TotalMinutes,
            TimeInputMode.Minutes => int.Parse(value),
            _ => throw new HhregException(string.Format(HhregMessages.InvalidInputFormatOnValue, value))
        };
    }
}