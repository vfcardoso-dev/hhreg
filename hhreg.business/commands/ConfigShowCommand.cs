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
        var cfg = _settingsRepository.Get()!;

        var table = new Table();
        table.AddColumns(cfg.CreateHeaders());
        table.AddRow(cfg.CreateRenderableRow());
        AnsiConsole.Write(table);
        return 0;
    }
}