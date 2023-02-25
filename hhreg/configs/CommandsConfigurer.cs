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

        cmd.AddCommand<InitCommand>("init")
            .WithDescription("Initializes CLI settings.")
            .WithExample(new string[]{"init","--initial-balance","-0:40","--work-day","8:00","--start-calculations-at","01/12/2022"})
            .WithExample(new string[]{"init","-m","Minutes","-b","-20","-w","480","-s","01/12/2022"});
        
        cmd.AddBranch("config", config => {
            config.SetDescription("Manage CLI settings");

            config.AddCommand<ConfigShowCommand>("show").WithDescription("Shows current settings.");
            config.AddCommand<ConfigDatabaseCommand>("database").WithDescription("Prints database location.");
            config.AddCommand<ConfigEditCommand>("edit")
                .WithDescription("Changes current settings.")
                .WithExample(new string[]{"config","edit","--initial-balance","1:20","--start-calculations-at","01/04/2019"})
                .WithExample(new string[]{"config","edit","-m","Minutes","-b","10"});
        });

        cmd.AddBranch("entry", entry => {
            entry.SetDescription("Manage entry logs.");

            entry.AddCommand<EntryNewCommand>("new")
                .WithDescription("Logs new entries.")
                .WithExample(new string[]{"entry","new","--day","23/02/2023","08:23","12:01","13:44","19:37"})
                .WithExample(new string[]{"entry","new","-d","12/01/2023","-y","Sick","-j","\"This is a justification\""});

            entry.AddCommand<EntryOverrideCommand>("override").WithDescription("Override entry logs.");
            entry.AddCommand<EntryNowCommand>("now").WithDescription("Log an entry now.");
        });

        cmd.AddBranch("report", report => {
            report.SetDescription("Summarize and show time entries");

            report.AddCommand<ReportDayCommand>("day").WithDescription("Show time entries for a single day.");
            report.AddCommand<ReportMonthCommand>("month").WithDescription("Show time entries for a month.");
            report.AddCommand<ReportBalanceCommand>("balance").WithDescription("Show accumulated hour bank balance.");
            report.AddCommand<ReportMyDrakeCommand>("mydrake").WithDescription("Export time entries to bulk insert on MyDrake.");
        });
    }
}