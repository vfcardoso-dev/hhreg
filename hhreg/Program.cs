using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace hhreg;
static class Program
{
    static int Main(string[] args)
    {
        var app = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(LoggingConfigurer.Configure)
            .ConfigureServices(ServicesConfigurer.Configure)
            .Build();

        try {
            return app.Services.GetRequiredService<AppHost>().Run(args);
        } catch (Exception ex) {
            AnsiConsole.MarkupLineInterpolated($"[red]ERRO:[/] {ex.Message}");
            return -99;
        }
    }
}