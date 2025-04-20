using Microsoft.Extensions.DependencyInjection;
using Streamnesia.Core;

namespace Streamnesia.Execution;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAmnesiaExecution(this IServiceCollection services)
    {
        services.AddSingleton<IAmnesiaClient, AmnesiaClient>();

        return services;
    }
}
