using System.Diagnostics.CodeAnalysis;
using hhreg.business.exceptions;
using hhreg.business.infrastructure;
using Spectre.Console.Cli;

namespace hhreg.business.commands;

public sealed class ConfigDatabaseCommand : Command<ConfigDatabaseCommand.Settings>
{
    private readonly ISettingsService _appSettings;
    private readonly ILogger _logger;

    public ConfigDatabaseCommand(ISettingsService appSettings, ILogger logger) {
        _appSettings = appSettings;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings {}

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        _logger.WriteFilePath(HhregMessages.DatabaseLocationTitle, _appSettings.DatabaseFilePath);
        return 0;
    }
}