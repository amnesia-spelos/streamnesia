using Streamnesia.Client.Options;
using Streamnesia.Execution;
using Streamnesia.Twitch;

namespace Streamnesia.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStreamnesiaDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AmnesiaClientConfig>(configuration.GetSection(nameof(AmnesiaClientConfig)));
        services.Configure<TwitchBotConfig>(configuration.GetSection(nameof(TwitchBotConfig)));

        services.AddAmnesiaExecution();
        services.AddTwitchBot();

        return services;
    }
}
