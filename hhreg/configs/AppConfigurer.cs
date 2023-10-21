using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace hhreg.configs;

public static class AppConfigurer 
{
    public static void Configure(HostBuilderContext context, IConfigurationBuilder builder) 
    {
        builder
            .SetBasePath(AppContext.BaseDirectory);
    }
}