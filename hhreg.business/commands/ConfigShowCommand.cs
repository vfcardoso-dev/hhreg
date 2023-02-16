using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace hhreg.business;

public sealed class ConfigShowCommand : Command<ConfigShowCommand.Settings>
{
    private readonly ISettingsRepository _settingsRepository;

    public ConfigShowCommand(ISettingsRepository settingsRepository) {
        _settingsRepository = settingsRepository;
    }

    public sealed class Settings : CommandSettings {}

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var cfg = _settingsRepository.GetOrDefault();

        var table = new Table();
        table.AddColumns("Saldo inicial", "Jornada", "Almoço");
        
        var initBalance = FormatTime(cfg.InitialBalance);
        var workDay = FormatTime(cfg.WorkDay);
        var lunchTime = FormatTime(cfg.LunchTime);
        
        table.AddRow(initBalance, workDay, lunchTime);

        AnsiConsole.Write(table);
        return 0;
    }

    private string FormatTime(double value) {
        var signal = value < 0 ? "-" : "";
        return $"{signal}{TimeSpan.FromMinutes(value).ToString("hh\\:mm")}";
    }
}