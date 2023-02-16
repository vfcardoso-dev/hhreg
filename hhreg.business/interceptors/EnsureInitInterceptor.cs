using Dapper;
using Spectre.Console.Cli;

namespace hhreg.business;

public interface IEnsureInitInterceptor : ICommandInterceptor {}

public class EnsureInitInterceptor : IEnsureInitInterceptor
{
    private readonly ISettingsRepository _settingsRepository;

    public EnsureInitInterceptor(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public void Intercept(CommandContext context, CommandSettings settings)
    {
        if (context.Name == "init") return;

        var initialized = _settingsRepository.IsAlreadyInitialized();

        if (!initialized) {
            throw new HhregException("É necessário inicializar as configurações com o comando 'init'.");
        }
    }
}