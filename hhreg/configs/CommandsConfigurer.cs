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
            .WithDescription("Inicializa as configurações.");
        
        config.AddBranch("config", cfg => {
            cfg.AddCommand<ConfigShowCommand>("show")
                .WithDescription("Mostra as configurações atuais.");
            cfg.AddCommand<ConfigEditCommand>("edit")
                .WithDescription("Altera as configurações atuais.");
            
            cfg.SetDescription("Gerencia as configurações");
        });
    }
}