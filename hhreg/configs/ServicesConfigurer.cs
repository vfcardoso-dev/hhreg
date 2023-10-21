using hhreg.business.infrastructure;
using hhreg.business.interceptors;
using hhreg.business.repositories;
using hhreg.resources;
using hhreg.services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
using TextCopy;

namespace hhreg.configs;

public static class ServicesConfigurer {
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