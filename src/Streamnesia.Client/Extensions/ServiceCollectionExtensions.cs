using Streamnesia.Configuration;
using Streamnesia.Execution;
using Streamnesia.Twitch;

namespace Streamnesia.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStreamnesiaDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAmnesiaExecution();
        services.AddTwitchBot();
        services.AddStreamnesiaConfiguration();

        return services;
    }
}
