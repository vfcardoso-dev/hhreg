using Microsoft.Extensions.Logging;

namespace hhreg;

public static class LoggingConfigurer {
    public static void Configure(ILoggingBuilder logging) 
    {
        logging.ClearProviders(); // removes all providers from LoggerFactory
        logging.AddSimpleConsole();  
    }
}