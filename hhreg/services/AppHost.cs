using hhreg.configs;
using Spectre.Console.Cli;

namespace hhreg.services;

public class AppHost {
    private readonly IDatabaseEnsurer _databaseEnsurer;
    private readonly ICommandsConfigurer _commandsConfigurer;
    private readonly ITypeRegistrar _typeRegistrar;

    public AppHost(
        IDatabaseEnsurer databaseEnsurer,
        ICommandsConfigurer commandsConfigurer, 
        ITypeRegistrar typeRegistrar) {
            _databaseEnsurer = databaseEnsurer;
            _commandsConfigurer = commandsConfigurer;
            _typeRegistrar = typeRegistrar;
    }

    public int Run(string[] args) {
        _databaseEnsurer.Ensure();

        var app = new CommandApp(_typeRegistrar);
        app.Configure(_commandsConfigurer.Configure);
        
        return app.Run(args);
    }
}