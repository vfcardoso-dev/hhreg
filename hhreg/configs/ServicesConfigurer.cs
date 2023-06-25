using hhreg.business;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using TextCopy;

namespace hhreg;
public static class ServicesConfigurer {
    public static void Configure(IServiceCollection services) 
    {
        services.AddSingleton<IAppSettings>(ctx => {
            var appSettings = new AppSettings();
            ctx.GetRequiredService<IConfiguration>().Bind("AppSettings", appSettings);
            return appSettings;
        });

        services.AddScoped(ctx => 
            new UnitOfWorkContext(
                ctx.GetRequiredService<IAppSettings>())
            .Create());

        services.AddScoped<IDatabaseEnsurer, DatabaseEnsurer>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<ITimeRepository, TimeRepository>();
        services.AddScoped<IClipboard, Clipboard>();
        
        // resto das dependencias....
        // ...

        services.AddSingleton<AppHost>();
        services.AddSingleton<ILogger, Logger>();
        services.AddScoped<IEnsureInitInterceptor, EnsureInitInterceptor>();
        services.AddScoped<ICommandsConfigurer, CommandsConfigurer>();
        services.AddSingleton<ITypeRegistrar>(new TypeRegistrar(services));
    }
}