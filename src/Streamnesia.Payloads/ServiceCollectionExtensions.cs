using Microsoft.Extensions.DependencyInjection;
using Streamnesia.Core;

namespace Streamnesia.Payloads;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPayloads(this IServiceCollection services)
    {
        services.AddSingleton<ICommandQueue, CommandQueue>();

        return services;
    }
}
