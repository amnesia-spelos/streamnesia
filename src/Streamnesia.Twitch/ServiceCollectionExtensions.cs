using Microsoft.Extensions.DependencyInjection;
using Streamnesia.Core;

namespace Streamnesia.Twitch;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTwitchBot(this IServiceCollection services)
    {
        services.AddSingleton<ITwitchBot, TwitchBot>();

        return services;
    }
}
