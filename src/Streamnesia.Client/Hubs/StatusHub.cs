using System.IO.Pipelines;
using Microsoft.AspNetCore.SignalR;
using Streamnesia.Core;
using Streamnesia.Core.Entities;

namespace Streamnesia.Client.Hubs;

public class StatusHub(
    IAmnesiaClient amnesiaClient,
    ICommandQueue queue,
    IPayloadLoader payloadLoader,
    ILocalPayloadConductor localConductor,
    ILogger<StatusHub> logger) : Hub
{
    public Task StartAmnesiaClient() => amnesiaClient.ConnectAsync();

    public Task StopAmnesiaClient() => amnesiaClient.Disconnect();

    public async Task LoadPayloadsTest()
    {
        var loadResult = await payloadLoader.LoadPayloadsAsync();

        if (loadResult.IsFailed)
        {
            logger.LogError("Result Failed: {Result}", string.Join(", ", loadResult.Errors.Select(e => e.Message)));
        }
        else
        {
            logger.LogInformation("Payloads loaded. Loaded {Count} payloads.", payloadLoader.Payloads?.Count);
        }
    }

    public async Task RunCommandQueueTest()
    {
        queue.AddPayload(new ParsedPayload
        {
            Name = "test payload 1",
            Sequence = [
                new ParsedPayloadSequenceItem
                {
                    AngelCode = "SetPlayerActive(false);",
                    Delay = TimeSpan.FromSeconds(0)
                },
                new ParsedPayloadSequenceItem
                {
                    AngelCode = "SetPlayerActive(true);",
                    Delay = TimeSpan.FromSeconds(1)
                },
                new ParsedPayloadSequenceItem
                {
                    AngelCode = "SetPlayerActive(false);",
                    Delay = TimeSpan.FromSeconds(2)
                }
            ]
        });
        queue.AddPayload(new ParsedPayload
        {
            Name = "test payload 2",
            Sequence = [
                new ParsedPayloadSequenceItem
                {
                    AngelCode = "SetPlayerActive(true);",
                    Delay = TimeSpan.FromSeconds(3)
                }
            ]
        });

        queue.Start();
        logger.LogInformation("Queue started");

        logger.LogInformation("Waiting for 10 seconds before stopping the queue");
        await Task.Delay(TimeSpan.FromSeconds(10));

        await queue.StopAsync();
        logger.LogInformation("We're done.");
    }

    public void StartLocalChaos()
    {
        var result = localConductor.Start();

        if (result.IsFailed)
        {
            logger.LogError("Result Failed: {Result}", string.Join(", ", result.Errors.Select(e => e.Message)));
            return;
        }

        logger.LogInformation("Local chaos running... Good luck!");
    }
}
