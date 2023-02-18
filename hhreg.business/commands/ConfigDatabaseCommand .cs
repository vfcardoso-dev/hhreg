using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace hhreg.business;

public sealed class ConfigDatabaseCommand : Command<ConfigDatabaseCommand.Settings>
{
    private readonly IAppSettings _appSettings;

    public ConfigDatabaseCommand(IAppSettings appSettings) {
        _appSettings = appSettings;
    }

    public sealed class Settings : CommandSettings {}

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var table = new Table();
        table.AddColumns(new TableColumn(new Text("Database location", new Style(Color.Green, Color.Black))));
        table.AddRow(new TextPath[]{ new TextPath(_appSettings.DatabaseFilePath) });
        AnsiConsole.Write(table);
        return 0;
    }
}