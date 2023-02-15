using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace hhreg;
public class AppHost {

    private readonly ILogger<AppHost> logger;

    public AppHost(ILoggerFactory loggerFactory) {
        logger = loggerFactory.CreateLogger<AppHost>();
    }

    public void Run(string[] args) {
        AnsiConsole.Markup("[underline cyan]Hello[/] World!");
        logger.LogInformation("Ahoy!");
    }
}