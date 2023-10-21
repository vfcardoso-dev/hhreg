using hhreg.business.exceptions;
using hhreg.business.infrastructure;
using Spectre.Console.Cli;

namespace hhreg.business.interceptors;

public interface IEnsureInitInterceptor : ICommandInterceptor {}

public class EnsureInitInterceptor : IEnsureInitInterceptor
{
    private readonly ISettingsService _settingsService;

    public EnsureInitInterceptor(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public void Intercept(CommandContext context, CommandSettings settings)
    {
        if (context.Name == "init") return;

        var initialized = _settingsService.IsInitialized();

        if (!initialized) {
            throw new HhregException("Você precisa inicializar as configurações do CLI através do comando '[green]init[/]'.");
        }
    }
}