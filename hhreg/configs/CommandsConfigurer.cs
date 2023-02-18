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

    public void Configure(IConfigurator config) 
    {
        config.PropagateExceptions();
        config.SetInterceptor(_ensureInitInterceptor);

        config.AddCommand<InitCommand>("init").WithDescription("Initializes CLI settings.");
        
        config.AddBranch("config", cfg => {
            cfg.SetDescription("Manage CLI settings");

            cfg.AddCommand<ConfigShowCommand>("show").WithDescription("Shows current settings.");
            cfg.AddCommand<ConfigEditCommand>("edit").WithDescription("Changes current settings.");
        });

        config.AddCommand<NewEntryCommand>("new").WithDescription("Logs new entries.");

        config.AddBranch("report", report => {
            report.SetDescription("Summarize and show time entries");

            report.AddCommand<ReportDayCommand>("day").WithDescription("Show time entries for a single day.");
        });
    }
}