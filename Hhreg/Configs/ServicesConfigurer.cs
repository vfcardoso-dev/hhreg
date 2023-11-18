using Hhreg.Business.Infrastructure;
using Hhreg.Business.Interceptors;
using Hhreg.Business.Repositories;
using Hhreg.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using TextCopy;

namespace Hhreg.Configs;

public static class ServicesConfigurer
{
    public static void Configure(IServiceCollection services)
    {
        services.AddLocalization();

        services.AddSingleton<ISettingsService, SettingsService>();

        services.AddScoped(ctx =>
            new UnitOfWorkContext(ctx.GetRequiredService<ISettingsService>()).Create());

        services.AddScoped<IDatabaseEnsurer, DatabaseEnsurer>();
        services.AddScoped<ITimeRepository, TimeRepository>();
        services.AddScoped<IClipboard, Clipboard>();

        // resto das dependencias....

        services.AddSingleton<AppHost>();
        services.AddSingleton<ILogger, Logger>();
        services.AddScoped<IEnsureInitInterceptor, EnsureInitInterceptor>();
        services.AddScoped<ICommandsConfigurer, CommandsConfigurer>();
        services.AddSingleton<ITypeRegistrar>(new TypeRegistrar(services));
    }
}