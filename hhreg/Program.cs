using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace hhreg;
static class Program
{
    static int Main(string[] args)
    {
        var app = Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(builder => HostConfigurer.Configure(builder, args))
            .ConfigureLogging(LoggingConfigurer.Configure)
            .ConfigureServices(ServicesConfigurer.Configure)
            .ConfigureAppConfiguration(AppConfigurer.Configure)
            .Build();

        var env = app.Services.GetRequiredService<IHostEnvironment>();

        try {
            if (!env.IsProduction()) {
                AnsiConsole.MarkupLineInterpolated($"[yellow]ENV:[/] {env.EnvironmentName}");
            }

            return app.Services.GetRequiredService<AppHost>().Run(args);
        } catch (Exception ex) {
            AnsiConsole.MarkupLine($"[red]ERROR:[/] {ex.Message}");
            
            if (!env.IsProduction()) {
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            }

            return -99;
        }
    }
}