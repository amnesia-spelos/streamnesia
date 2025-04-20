using Microsoft.Extensions.Options;
using Streamnesia.Client.Options;
using Streamnesia.Core.Configuration;
using Streamnesia.Execution;
using Streamnesia.Twitch;

namespace Streamnesia.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStreamnesiaDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AmnesiaClientConfig>(configuration.GetSection(nameof(AmnesiaClientConfig)));
        services.AddSingleton<IAmnesiaClientConfig>(sp => sp.GetRequiredService<IOptions<AmnesiaClientConfig>>().Value);

        services.Configure<TwitchBotConfig>(configuration.GetSection(nameof(TwitchBotConfig)));
        services.AddSingleton<ITwitchBotConfig>(sp => sp.GetRequiredService<IOptions<TwitchBotConfig>>().Value);

        services.AddAmnesiaExecution();
        services.AddTwitchBot();

        return services;
    }
}
