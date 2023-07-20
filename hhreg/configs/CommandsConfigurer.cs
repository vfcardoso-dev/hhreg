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
            .WithDescription(_localizer.Get("InitCommandDescription"))
            .WithExample(new string[]{"init","--initial-balance","-0:40","--workday","8:00","--start-calculations-at","01/12/2022"})
            .WithExample(new string[]{"init","-m","Minutes","-b","-20","-w","480","-s","01/12/2022"});

        cmd.AddCommand<UpdateCommand>("update")
            .WithDescription(_localizer.Get("UpdateCommandDescription"));
        
        cmd.AddBranch("config", config => {
            config.SetDescription(_localizer.Get("ConfigCommandDescription"));

            config.AddCommand<ConfigShowCommand>("show")
                .WithDescription(_localizer.Get("ConfigShowCommandDescription"));
            config.AddCommand<ConfigDatabaseCommand>("database")
                .WithDescription(_localizer.Get("ConfigDatabaseCommandDescription"));
            config.AddCommand<ConfigEditCommand>("edit")
                .WithDescription(_localizer.Get("ConfigEditCommandDescription"))
                .WithExample(new string[]{"config","edit","--initial-balance","1:20","--start-calculations-at","01/04/2019"})
                .WithExample(new string[]{"config","edit","-m","Minutes","-b","10"});
        });

        cmd.AddBranch("entry", entry => {
            entry.SetDescription(_localizer.Get("EntryCommandDescription"));

            entry.AddCommand<EntryNewCommand>("new")
                .WithDescription(_localizer.Get("EntryNewCommandDescription"))
                .WithExample(new string[]{"entry","new","--day","23/02/2023","08:23","12:01","13:44","19:37"})
                .WithExample(new string[]{"entry","new","-d","12/01/2023","-y","Sick","-j","\"This is a justification\""});

            entry.AddCommand<EntryOverrideCommand>("override")
                .WithDescription(_localizer.Get("EntryOverrideCommandDescription"))
                .WithExample(new string[]{"entry","override","--day","23/02/2023","08:23","12:01","13:44","19:37"})
                .WithExample(new string[]{"entry","override","-d","12/01/2023","-y","Sick","-j","\"This is a justification\""});

            entry.AddCommand<EntryNowCommand>("now")
                .WithDescription(_localizer.Get("EntryNowCommandDescription"));
        });

        cmd.AddBranch("report", report => {
            report.SetDescription(_localizer.Get("ReportCommandDescription"));

            report.AddCommand<ReportDayCommand>("day")
                .WithDescription(_localizer.Get("ReportDayCommandDescription"))
                .WithExample(new string[]{"report","day","23/02/2023"});
            report.AddCommand<ReportMonthCommand>("month")
                .WithDescription(_localizer.Get("ReportMonthCommandDescription"))
                .WithExample(new string[]{"report","month","02/2023"});
            report.AddCommand<ReportBalanceCommand>("balance")
                .WithDescription(_localizer.Get("ReportBalanceCommandDescription"))
                .WithExample(new string[]{"report","balance"})
                .WithExample(new string[]{"report","balance","--tail","30"});
            report.AddCommand<ReportMyDrakeCommand>("mydrake")
                .WithDescription(_localizer.Get("ReportMyDrakeCommandDescription"))
                .WithExample(new string[]{"report","mydrake","01/07/2023"})
                .WithExample(new string[]{"report","mydrake","01/03/2023","31/03/2023"});
        });
    }
}