using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace hhreg;
public class DatabaseEnsurer {

    private readonly IConfiguration _config;
    private readonly ILogger<DatabaseEnsurer> _logger;

    public DatabaseEnsurer(IConfiguration config, ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<DatabaseEnsurer>();
        _config = config;
    }

    public void Ensure() {
        
    }
}