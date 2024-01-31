using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Hhreg.Business.Domain.ValueObjects;
using Hhreg.Business.Exceptions;
using Hhreg.Business.Infrastructure;
using Hhreg.Business.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Hhreg.Business.Commands;

public sealed class InitCommand : Command<InitCommand.Settings>
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger _logger;

    public InitCommand(ISettingsService settingsService, ILogger logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Saldo inicial do banco de horas")]
        [CommandOption("-b|--initial-balance")]
        public string? InitialBalance { get; init; }

        [Description("Duração do dia de trabalho")]
        [CommandOption("-w|--workday")]
        public string? WorkDay { get; init; }

        [Description("Modo de inserção de tempo: Hours (HH:mm) ou Minutes (1..999+). Padrão: Hours")]
        [CommandOption("-m|--time-input-mode")]
        [DefaultValue(TimeInputMode.Hours)]
        public TimeInputMode TimeInputMode { get; init; }

        [Description("Data de início do cálculo de banco de horas (formato: dd/MM/yyyy)")]
        [CommandOption("-s|--start-calculations-at")]
        public string? StartCalculationsAt { get; init; }

        [Description("Tolerância na marcação")]
        [CommandOption("-t|--tolerance")]
        public string? EntryTolerance { get; init; }

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

            if (TimeInputMode == TimeInputMode.Hours && EntryTolerance?.IsTime() == false)
                return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidTimeFormat, WorkDay));

            if (TimeInputMode == TimeInputMode.Minutes && EntryTolerance?.IsInteger() == false)
                return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidIntegerFormat, WorkDay));

            if (!DateOnly.TryParse(StartCalculationsAt, out var _))
                return ValidationResult.Error(string.Format(HhregMessages.CouldNotParseAsAValidDateFormat, StartCalculationsAt));

            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        if (_settingsService.IsInitialized())
        {
            throw new HhregException(HhregMessages.SettingsAlreadyInitialized);
        }

        var initialBalance = GetTimeInputValue(settings.TimeInputMode, settings.InitialBalance!);
        var workday = GetTimeInputValue(settings.TimeInputMode, settings.WorkDay!);
        var entryTolerance = settings.EntryTolerance != null 
            ? GetTimeInputValue(settings.TimeInputMode, settings.EntryTolerance!) 
            : 0;
        var startCalculationsAt = DateOnly.Parse(settings.StartCalculationsAt!).ToString("yyyy-MM-dd");

        _settingsService.SaveSettings(new Domain.Settings
        {
            StartBalanceInMinutes = initialBalance,
            WorkDayInMinutes = workday,
            EntryToleranceInMinutes = entryTolerance,
            LastBalanceCutoff = startCalculationsAt
        });

        _logger.WriteLine($@"Configurações inicializadas com [green]SUCESSO[/]!");
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