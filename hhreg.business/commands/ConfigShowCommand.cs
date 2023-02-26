using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace hhreg.business;

public sealed class ConfigShowCommand : Command<ConfigShowCommand.Settings>
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger _logger;

    public ConfigShowCommand(ISettingsRepository settingsRepository, ILogger logger) {
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings {}

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var cfg = _settingsRepository.Get()!;

        _logger.WriteTable(cfg.ExtractColumns(), new List<string[]>{{cfg.ExtractRow()}});
        return 0;
    }
}