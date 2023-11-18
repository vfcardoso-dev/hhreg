using System.Diagnostics.CodeAnalysis;
using Hhreg.Business.Exceptions;
using Hhreg.Business.Infrastructure;
using Spectre.Console.Cli;

namespace Hhreg.Business.Commands;

public sealed class ConfigDatabaseCommand : Command<ConfigDatabaseCommand.Settings>
{
    private readonly ISettingsService _appSettings;
    private readonly ILogger _logger;

    public ConfigDatabaseCommand(ISettingsService appSettings, ILogger logger)
    {
        _appSettings = appSettings;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings { }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        _logger.WriteFilePath(HhregMessages.DatabaseLocationTitle, _appSettings.DatabaseFilePath);
        return 0;
    }
}