using Microsoft.Extensions.Logging;

namespace hhreg;
public class AppHost {

    private readonly ILogger<AppHost> _logger;
    private readonly IDatabaseEnsurer _databaseEnsurer;

    public AppHost(ILoggerFactory loggerFactory, IDatabaseEnsurer databaseEnsurer) {
        _logger = loggerFactory.CreateLogger<AppHost>();
        _databaseEnsurer = databaseEnsurer;
    }

    public void Run(string[] args) {
        _databaseEnsurer.Ensure();

        _logger.LogInformation("Olá, mundo");
    }
}