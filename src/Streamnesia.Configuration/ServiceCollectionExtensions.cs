using Microsoft.Extensions.DependencyInjection;
using Streamnesia.Core;

namespace Streamnesia.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStreamnesiaConfiguration(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystemPaths, FileSystemPaths>();
        services.AddSingleton(typeof(IStoredItem<>), typeof(JsonFileStoredItem<>));
        services.AddSingleton<IConfigurationStorage, ConfigurationStorage>();

        return services;
    }
}
