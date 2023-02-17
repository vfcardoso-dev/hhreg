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

        config.AddCommand<InitCommand>("init")
            .WithDescription("Initialize CLI settings.");
        
        config.AddBranch("config", cfg => {
            cfg.AddCommand<ConfigShowCommand>("show")
                .WithDescription("Shows current settings.");
            cfg.AddCommand<ConfigEditCommand>("edit")
                .WithDescription("Change current settings.");
            
            cfg.SetDescription("Manage CLI settings");
        });
    }
}