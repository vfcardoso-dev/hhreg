using hhreg.business;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace hhreg;
public static class ServicesConfigurer {
    public static void Configure(IServiceCollection services) 
    {
        services.AddSingleton<AppSettings>(ctx => {
            var appSettings = new AppSettings();
            ctx.GetRequiredService<IConfiguration>().Bind("AppSettings", appSettings);
            return appSettings;
        });

        services.AddScoped<IUnitOfWork>(ctx => 
            new UnitOfWorkContext(
                ctx.GetRequiredService<AppSettings>(), 
                ctx.GetRequiredService<ILoggerFactory>())
            .Create());

        services.AddScoped<IDatabaseEnsurer, DatabaseEnsurer>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        
        // resto das dependencias....
        // ...

        services.AddSingleton<AppHost>();
        services.AddScoped<IEnsureInitInterceptor, EnsureInitInterceptor>();
        services.AddScoped<ICommandsConfigurer, CommandsConfigurer>();
        services.AddSingleton<ITypeRegistrar>(new TypeRegistrar(services));
    }
}