using Microsoft.AspNetCore.SignalR;
using Streamnesia.Core;
using Streamnesia.Core.Entities;

namespace Streamnesia.Client.Hubs;

public class StatusHub(IAmnesiaClient amnesiaClient, ICommandQueue queue, IPayloadLoader payloadLoader, ILogger<StatusHub> logger) : Hub
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
        queue.AddPayload(new PayloadModel
        {
            Name = "test payload 1",
            Sequence = [
                new PayloadSequenceModel
                {
                    File = "D:\\dev\\scripts\\spawn-barrel.payload",
                    Delay = TimeSpan.FromSeconds(0)
                },
                new PayloadSequenceModel
                {
                    File = "D:\\dev\\scripts\\spawn-barrel.payload",
                    Delay = TimeSpan.FromSeconds(1)
                },
                new PayloadSequenceModel
                {
                    File = "D:\\dev\\scripts\\spawn-barrel.payload",
                    Delay = TimeSpan.FromSeconds(2)
                }
            ]
        });
        queue.AddPayload(new PayloadModel
        {
            Name = "test payload 2",
            Sequence = [
                new PayloadSequenceModel
                {
                    File = "D:\\dev\\scripts\\spawn-barrel.payload",
                    Delay = TimeSpan.FromSeconds(0)
                }
            ]
        });

        queue.Start();

        logger.LogInformation("Queue started, waiting for 5 seconds before adding concurrently...");
        await Task.Delay(TimeSpan.FromSeconds(5));

        var tasks = new List<Task>
        {
            Task.Run(() => queue.AddPayload(new PayloadModel
            {
                Name = "concurrent payload 1",
                Sequence = [
                    new PayloadSequenceModel
                    {
                        File = "D:\\dev\\scripts\\spawn-barrel.payload",
                        Delay = TimeSpan.FromMilliseconds(100)
                    }
                ]
            })),

            Task.Run(() => queue.AddPayload(new PayloadModel
            {
                Name = "concurrent payload 2",
                Sequence = [
                    new PayloadSequenceModel
                    {
                        File = "D:\\dev\\scripts\\spawn-barrel.payload",
                        Delay = TimeSpan.FromMilliseconds(100)
                    }
                ]
            })),

            Task.Run(() => queue.AddPayload(new PayloadModel
            {
                Name = "concurrent payload 3",
                Sequence = [
                    new PayloadSequenceModel
                    {
                        File = "D:\\dev\\scripts\\spawn-barrel.payload",
                        Delay = TimeSpan.FromMilliseconds(300)
                    }
                ]
            }))
        };

        await Task.WhenAll(tasks);

        logger.LogInformation("All concurrent payloads added.");

        logger.LogInformation("Waiting for 10 seconds before stopping the queue");
        await Task.Delay(TimeSpan.FromSeconds(10));

        await queue.StopAsync();
        logger.LogInformation("We're done.");
    }
}
