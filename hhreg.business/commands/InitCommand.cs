using System.ComponentModel;
using Spectre.Console.Cli;
using Spectre.Console;
using System.Diagnostics.CodeAnalysis;

namespace hhreg.business;

public sealed class InitCommand : Command<InitCommand.Settings>
{
    private readonly ISettingsRepository _settingsRepository;

    public InitCommand(ISettingsRepository settingsRepository) {
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
            if (InitialBalanceMinutes == null && InitialBalanceHours == null) {
                return ValidationResult.Error("Você deve informar um saldo inicial do banco de horas (em minutos ou horas).");
            }
                
            if (WorkDayMinutes == null && WorkDayHours == null) {
                return ValidationResult.Error("Você deve informar a jornada de trabalho (em minutos ou horas).");
            }
            
            return ValidationResult.Success();
        }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        if (_settingsRepository.IsAlreadyInitialized()) {
            throw new HhregException("Configuração já inicializada. Você pode editar com 'config edit'");
        }
        
        var initialBalance = settings.InitialBalanceMinutes ?? ExtractMinutes(settings.InitialBalanceHours!);
        var workDay = settings.WorkDayMinutes ?? ExtractMinutes(settings.WorkDayHours!);

        _settingsRepository.Create(initialBalance, workDay);
        
        AnsiConsole.MarkupLineInterpolated($@"Configurações inicializadas com [green]SUCESSO[/]!");
        return 0;
    }

    private double ExtractMinutes(string time) {
        return TimeSpan.Parse(time).TotalMinutes;
    }
}