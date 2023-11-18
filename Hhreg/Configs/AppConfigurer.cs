using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Hhreg.Configs;

public static class AppConfigurer
{
    public static void Configure(HostBuilderContext context, IConfigurationBuilder builder)
    {
        builder
            .SetBasePath(AppContext.BaseDirectory);
    }
}