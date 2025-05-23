﻿using Microsoft.Extensions.DependencyInjection;
using Streamnesia.Core;

namespace Streamnesia.Payloads;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPayloads(this IServiceCollection services)
    {
        services.AddSingleton<ICommandPreprocessor, CommandPreprocessor>();
        services.AddSingleton<ICommandQueue, CommandQueue>();
        services.AddSingleton<IPayloadLoader, PayloadLoader>();

        return services;
    }
}
