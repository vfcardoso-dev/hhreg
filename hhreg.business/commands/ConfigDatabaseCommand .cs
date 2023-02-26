using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace hhreg.business;

public sealed class ConfigDatabaseCommand : Command<ConfigDatabaseCommand.Settings>
{
    private readonly IAppSettings _appSettings;
    private readonly ILogger _logger;

    public ConfigDatabaseCommand(IAppSettings appSettings, ILogger logger) {
        _appSettings = appSettings;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings {}

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        _logger.WriteFilePath("Database location", _appSettings.DatabaseFilePath);
        return 0;
    }
}