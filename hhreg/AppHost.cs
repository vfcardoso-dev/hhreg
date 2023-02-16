using Microsoft.Extensions.Logging;

namespace hhreg;
public class AppHost {

    private readonly ILogger<AppHost> _logger;

    public AppHost(ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<AppHost>();
    }

    public void Run(string[] args) {
        _logger.LogInformation("Olá, mundo");
    }
}