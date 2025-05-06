using System;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Streamnesia.Core;
using Streamnesia.Core.Conductors;
using Streamnesia.Execution.Conductors;

namespace Streamnesia.Execution;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAmnesiaExecution(this IServiceCollection services)
    {
        services.AddSingleton<Random>();
        services.AddSingleton<TcpClient>();
        services.AddSingleton<IAmnesiaClient, AmnesiaClient>();
        services.AddSingleton<ILocalPayloadConductor, LocalPayloadConductor>();
        services.AddSingleton<ITwitchPollConductor, TwitchPollConductor>();
        services.AddSingleton<IDevelopmentConductor, DevelopmentConductor>();
        services.AddSingleton(typeof(IPoll<>), typeof(Poll<>));

        return services;
    }
}
