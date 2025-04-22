using Streamnesia.Configuration;
using Streamnesia.Execution;
using Streamnesia.Twitch;
using Streamnesia.Payloads;

namespace Streamnesia.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStreamnesiaDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAmnesiaExecution();
        services.AddTwitchBot();
        services.AddStreamnesiaConfiguration();
        services.AddPayloads();

        return services;
    }
}
