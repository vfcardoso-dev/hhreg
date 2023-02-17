using Microsoft.Extensions.Configuration;

namespace hhreg;

public static class HostConfigurer
{
    public static void Configure(IConfigurationBuilder builder, string[] args)
    {
        // enviroment from command line. e.g.: dotnet run --environment "Staging"
        builder.AddCommandLine(args ?? new string[]{});
    }
}