using hhreg.business.utilities;
using hhreg.configs;
using hhreg.services;
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
            .ConfigureServices(ServicesConfigurer.Configure)
            .ConfigureAppConfiguration(AppConfigurer.Configure)
            .Build();

        try {
            var appHost = app.Services.GetRequiredService<AppHost>();
            var notCalculatedArgs = new string[]{"-h","-v","--help","--version", "update"};
            
            if (args.IsEmpty() || args.Any(arg => notCalculatedArgs.Contains(arg))) {
                return appHost.Run(args);
            }

            return AnsiConsole.Status()
                .Spinner(Spinner.Known.Star2)
                .SpinnerStyle(Style.Parse("green"))
                .Start("Calculating...", ctx => appHost.Run(args));
                
        } catch (Exception ex) {
            AnsiConsole.MarkupLine($"[red]FATAL:[/] {ex.Message}");
            
            if (!app.Services.GetRequiredService<IHostEnvironment>().IsProduction()) {
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            }

            return -99;
        }
    }
}