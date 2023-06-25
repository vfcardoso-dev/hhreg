using Microsoft.Extensions.Configuration;

namespace hhreg.configs;

public static class HostConfigurer
{
    public static void Configure(IConfigurationBuilder builder, string[] args)
    {
        // enviroment from command line. e.g.: dotnet run --environment "Staging"
        _ = builder.AddCommandLine(args ?? Array.Empty<string>());
    }
}