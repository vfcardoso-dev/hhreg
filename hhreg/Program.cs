using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace hhreg;
class Program
{
    static void Main(string[] args)
    {
        var config = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logBuilder =>
                {
                    logBuilder.ClearProviders(); // removes all providers from LoggerFactory
                    logBuilder.AddConsole();  
                })
                .ConfigureServices(sp => {
                    sp.AddScoped<IDatabaseEnsurer, DatabaseEnsurer>();
                    sp.AddSingleton<AppHost>();
                    // resto das dependencias....
                })
                .Build();

        config.Services.GetService<AppHost>()?.Run(args);
    }
}

