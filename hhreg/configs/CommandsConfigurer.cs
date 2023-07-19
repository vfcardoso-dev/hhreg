using hhreg.business.commands;
using hhreg.business.interceptors;
using hhreg.resources;
using Spectre.Console.Cli;

namespace hhreg.configs;

public interface ICommandsConfigurer {
    void Configure(IConfigurator config);
}

public class CommandsConfigurer : ICommandsConfigurer {
    private readonly IEnsureInitInterceptor _ensureInitInterceptor;
    private readonly ILocalizer _localizer;

    public CommandsConfigurer(
        IEnsureInitInterceptor ensureInitInterceptor,
        ILocalizer localizer)
    {
        _ensureInitInterceptor = ensureInitInterceptor;
        _localizer = localizer;
    }

    public void Configure(IConfigurator cmd) 
    {
        cmd.PropagateExceptions();
        cmd.SetApplicationName("hhreg");
        cmd.CaseSensitivity(CaseSensitivity.None);
        cmd.ValidateExamples();

        cmd.SetInterceptor(_ensureInitInterceptor);

        cmd.AddCommand<InitCommand>("init")
            .WithDescription(_localizer.Get("InitCliSettings"))
            .WithExample(new string[]{"init","--initial-balance","-0:40","--workday","8:00","--start-calculations-at","01/12/2022"})
            .WithExample(new string[]{"init","-m","Minutes","-b","-20","-w","480","-s","01/12/2022"});

        cmd.AddCommand<UpdateCommand>("update")
            .WithDescription("Update app if new version is available.");
        
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

            entry.AddCommand<EntryOverrideCommand>("override")
                .WithDescription("Override entry logs.")
                .WithExample(new string[]{"entry","override","--day","23/02/2023","08:23","12:01","13:44","19:37"})
                .WithExample(new string[]{"entry","override","-d","12/01/2023","-y","Sick","-j","\"This is a justification\""});

            entry.AddCommand<EntryNowCommand>("now").WithDescription("Log an entry now.");
        });

        cmd.AddBranch("report", report => {
            report.SetDescription("Summarize and show time entries");

            report.AddCommand<ReportDayCommand>("day").WithDescription("Show time entries for a single day.")
                .WithExample(new string[]{"report","day","23/02/2023"});
            report.AddCommand<ReportMonthCommand>("month").WithDescription("Show time entries for a month.")
                .WithExample(new string[]{"report","month","02/2023"});
            report.AddCommand<ReportBalanceCommand>("balance").WithDescription("Show accumulated hour bank balance.")
                .WithExample(new string[]{"report","balance"})
                .WithExample(new string[]{"report","balance","--tail","30"});
            report.AddCommand<ReportMyDrakeCommand>("mydrake").WithDescription("Export time entries to bulk insert on MyDrake.")
                .WithExample(new string[]{"report","mydrake","01/07/2023"})
                .WithExample(new string[]{"report","mydrake","01/03/2023","31/03/2023"});
        });
    }
}