using System.Diagnostics.CodeAnalysis;
using hhreg.business.infrastructure;
using hhreg.business.repositories;
using Spectre.Console.Cli;

namespace hhreg.business.commands;

public sealed class ConfigShowCommand : Command<ConfigShowCommand.Settings>
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger _logger;

    public ConfigShowCommand(ISettingsService settingsService, ILogger logger) {
        _settingsService = settingsService;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings {}

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var cfg = _settingsService.GetSettings();

        _logger.WriteTable(cfg.ExtractColumns(), new List<string[]>{{cfg.ExtractRow()}});
        return 0;
    }
}