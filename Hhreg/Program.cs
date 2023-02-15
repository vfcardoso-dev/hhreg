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
                lb.AddSimpleConsole();
            });

        // Add services
        serviceCollection.AddSingleton<AppHost>();
        // Add other dependencies here ...                

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var appHost = serviceProvider.GetService<AppHost>();

        appHost?.Run(args);
    }
}

