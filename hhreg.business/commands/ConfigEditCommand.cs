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
        [Description("Saldo inicial do banco de horas (em minutos)")]
        [CommandOption("--saldo-minutos")]
        public double? InitialBalanceMinutes { get; init; }

        [Description("Jornada de trabalho (em minutos)")]
        [CommandOption("--jornada-minutos")]
        public double? WorkDayMinutes { get; init; }

        [Description("Saldo inicial do banco de horas (em horas)")]
        [CommandOption("--saldo-horas")]
        public string? InitialBalanceHours { get; init; }

        [Description("Jornada de trabalho (em horas)")]
        [CommandOption("--jornada-horas")]
        public string? WorkDayHours { get; init; }

        public override ValidationResult Validate()
        {
            if (InitialBalanceMinutes == null && InitialBalanceHours == null) 
                return ValidationResult.Error("Você deve informar um saldo inicial do banco de horas (em minutos ou horas).");
            if (WorkDayMinutes == null && WorkDayHours == null) 
                return ValidationResult.Error("Você deve informar a jornada de trabalho (em minutos ou horas).");
            
            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var initialBalance = settings.InitialBalanceMinutes ?? ExtractMinutes(settings.InitialBalanceHours!);
        var workDay = settings.WorkDayMinutes ?? ExtractMinutes(settings.WorkDayHours!);

        _settingsRepository.Update(initialBalance, workDay);
        
        AnsiConsole.MarkupLineInterpolated($@"Configurações alteradas com [green]SUCESSO[/]!");
        return 0;
    }

    private double ExtractMinutes(string time) {
        return TimeSpan.Parse(time).TotalMinutes;
    }
}

