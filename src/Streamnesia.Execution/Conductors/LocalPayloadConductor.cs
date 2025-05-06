using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using Streamnesia.Core;
using Streamnesia.Core.Configuration;
using System.Collections;
using Streamnesia.Core.Entities;
using System.Collections.Generic;
using Streamnesia.Core.Conductors;

namespace Streamnesia.Execution.Conductors;

public class LocalPayloadConductor(
    IAmnesiaClient amnesiaClient,
    IPayloadLoader payloadLoader,
    ICommandQueue commandQueue,
    IConfigurationStorage cfgStorage,
    ILogger<LocalPayloadConductor> logger) : ILocalPayloadConductor
{
    private LocalChaosConfig? _config;
    private CancellationTokenSource? _loopCts;
    private Task? _loopTask;

    private readonly Queue<ParsedPayload> _payloadQueue = new();

    public bool IsRunning { get; private set; }

    public Result Start()
    {
        _config = cfgStorage.ReadLocalChaosConfig();

        if (_config is null)
        {
            logger.LogError("Failed to read config");
            return Result.Fail("Failed to read config");
        }

        if (payloadLoader.Payloads is null || payloadLoader.Payloads.Count == 0)
        {
            logger.LogError("Cannot start: No payloads loaded");
            return Result.Fail("Cannot start: No payloads loaded");
        }

        if (!amnesiaClient.IsConnected)
        {
            logger.LogError("Cannot start: Amnesia client not connected");
            return Result.Fail("Cannot start: Amnesia client not connected");
        }

        commandQueue.Start();
        logger.LogInformation("Command queue started");

        FillPayloadQueue();

        IsRunning = true;
        _loopCts = new();
        _loopTask = RunLoopAsync(_loopCts.Token);

        return Result.Ok();
    }

    private void FillPayloadQueue()
    {
        if (_config is null || payloadLoader.Payloads is null)
        {
            logger.LogError("Failed to fill up the payload queue");
            return;
        }    

        var payloads = _config.IsSequential
            ? [.. payloadLoader.Payloads]
            : payloadLoader.Payloads.OrderBy(_ => Guid.NewGuid()).ToList();

        foreach (var payload in payloads)
        {
            _payloadQueue.Enqueue(payload);
        }
    }

    public Result Stop()
    {
        if (_loopCts is not null)
        {
            _loopCts.Cancel();
            _loopCts.Dispose();
            _loopCts = null;
        }

        logger.LogInformation("Chaos mode loop stopped");
        return Result.Ok();
    }

    private async Task RunLoopAsync(CancellationToken token)
    {
        if (payloadLoader.Payloads is null || payloadLoader.Payloads.Count == 0)
            return;

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_config?.IntervalInSeconds ?? 10)); // TODO: FIXME: better fallback? Live config update?

        while (await timer.WaitForNextTickAsync(token))
        {
            logger.LogDebug("Chaos timer tick");

            try
            {
                if (_payloadQueue.Count < 1)
                {
                    FillPayloadQueue();
                }

                var randomPayload = _payloadQueue.Dequeue();

                if (randomPayload is not null)
                {
                    logger.LogDebug("Payload Queue contains {Count} item(s).", _payloadQueue.Count);
                    logger.LogInformation("Adding random payload: {PayloadName}", randomPayload.Name);
                    commandQueue.AddPayload(randomPayload);
                }
                else
                {
                    logger.LogWarning("No payload selected. Payloads list may be empty.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occurred during chaos payload loop.");
            }
        }

        IsRunning = false;
    }
}
