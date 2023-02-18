using hhreg.business;
using Spectre.Console.Cli;

namespace hhreg;

public interface ICommandsConfigurer {
    void Configure(IConfigurator config);
}

public class CommandsConfigurer : ICommandsConfigurer {
    private readonly IEnsureInitInterceptor _ensureInitInterceptor;

    public CommandsConfigurer(IEnsureInitInterceptor ensureInitInterceptor)
    {
        _ensureInitInterceptor = ensureInitInterceptor;
    }

    public void Configure(IConfigurator cmd) 
    {
        cmd.PropagateExceptions();
        cmd.SetInterceptor(_ensureInitInterceptor);

        cmd.AddCommand<InitCommand>("init").WithDescription("Initializes CLI settings.");
        
        cmd.AddBranch("config", config => {
            config.SetDescription("Manage CLI settings");

            config.AddCommand<ConfigShowCommand>("show").WithDescription("Shows current settings.");
            config.AddCommand<ConfigEditCommand>("edit").WithDescription("Changes current settings.");
            config.AddCommand<ConfigDatabaseCommand>("database").WithDescription("Prints database location.");
        });

        cmd.AddBranch("entry", entry => {
            entry.SetDescription("Manage entry logs.");

            entry.AddCommand<EntryNewCommand>("new").WithDescription("Logs new entries.");
        });

        cmd.AddBranch("report", report => {
            report.SetDescription("Summarize and show time entries");

            report.AddCommand<ReportDayCommand>("day").WithDescription("Show time entries for a single day.");
        });
    }
}