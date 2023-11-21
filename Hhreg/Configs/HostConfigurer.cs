using Microsoft.Extensions.Configuration;

namespace Hhreg.Configs;

public static class HostConfigurer
{
    public static void Configure(IConfigurationBuilder builder, string[] args)
    {
        _ = builder
                .AddCommandLine(args ?? Array.Empty<string>())
                .AddJsonFile("appsettings.json", optional: true);
    }
}