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
    public static void Configure(HostBuilderContext context, IServiceCollection services) 
    {
        services.AddLocalization();

        services.AddSingleton<IDbSettings, DbSettings>();
        services.Configure<DbSettings>(context.Configuration.GetSection("AppSettings:Database"));

        services.AddScoped<ILocaleSettings, LocaleSettings>();
        services.Configure<LocaleSettings>(context.Configuration.GetSection("AppSettings:Localizer"));

        services.AddScoped(ctx => 
            new UnitOfWorkContext(ctx.GetRequiredService<IDbSettings>())
                .Create());

        services.AddScoped<IDatabaseEnsurer, DatabaseEnsurer>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<ITimeRepository, TimeRepository>();
        services.AddScoped<IClipboard, Clipboard>();
        
        // resto das dependencias....

        services.AddSingleton<AppHost>();
        services.AddSingleton<ILogger, Logger>();
        services.AddSingleton<ILocalizer, Localizer>();
        services.AddScoped<IEnsureInitInterceptor, EnsureInitInterceptor>();
        services.AddScoped<ICommandsConfigurer, CommandsConfigurer>();
        services.AddSingleton<ITypeRegistrar>(new TypeRegistrar(services));
    }
}