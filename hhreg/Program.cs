using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace hhreg;
class Program
{
    static void Main(string[] args)
    {
        // Configure logging
        var serviceCollection = new ServiceCollection()
            .AddLogging(lb =>
            {
                lb.ClearProviders();
                lb.AddConsole();
            });

        // Add services
        serviceCollection.AddSingleton<AppHost>();
        serviceCollection.AddSingleton<DatabaseEnsurer>();
        // Add other dependencies here ...                

        var serviceProvider = serviceCollection.BuildServiceProvider();

        
        serviceProvider.GetService<AppHost>()?.Run(args);
    }
}

