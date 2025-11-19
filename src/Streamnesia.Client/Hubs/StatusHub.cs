using Microsoft.AspNetCore.SignalR;
using Streamnesia.Client.Models;
using Streamnesia.Core;
using Streamnesia.Core.Conductors;

namespace Streamnesia.Client.Hubs;

public class StatusHub(
    IAmnesiaClient amnesiaClient,
    ICommandQueue queue,
    IPayloadLoader payloadLoader,
    ILocalPayloadConductor localConductor,
    ITwitchPollConductor twitchConductor,
    ITwitchBot twitchBot,
    ILogger<StatusHub> logger) : Hub
{
    public async Task StartAmnesiaClient()
    {
        await Clients.All.SendAsync("AmnesiaStateChanged", UiWidgetState.Progress);
        await amnesiaClient.ConnectAsync();
    }

    public Task StopAmnesiaClient() => amnesiaClient.Disconnect();

    public async Task LoadPayloads()
    {
        if (twitchBot.IsConnected)
        {
            twitchBot.Stop();
        }

        if (amnesiaClient.IsConnected)
        {
            await amnesiaClient.Disconnect();
        }

        await Clients.All.SendAsync("PayloadStateChanged", UiWidgetState.Progress.ToString());
        var loadResult = await payloadLoader.LoadPayloadsAsync();

        if (loadResult.IsFailed)
        {
            var reasons = string.Join(", ", loadResult.Errors.Select(e => e.Message));
            logger.LogError("Result Failed: {Result}", reasons);
            await Clients.All.SendAsync("PayloadLoadingFailed", reasons);
        }
        else
        {
            logger.LogInformation("Payloads loaded. Loaded {Count} payloads.", payloadLoader.Payloads?.Count);
            await Clients.All.SendAsync("PayloadStateChanged", UiWidgetState.Success.ToString());
        }
    }

    public async Task StartLocalChaos()
    {
        var loadResult = await payloadLoader.LoadPayloadsAsync();

        if (loadResult.IsFailed)
        {
            var error = string.Join(", ", loadResult.Errors.Select(e => e.Message));
            logger.LogError("Payload Load Failed: {Error}", error);
            await Clients.All.SendAsync("ChaosError", error);
            return;
        }

        var result = localConductor.Start();

        if (result.IsFailed)
        {
            var error = string.Join(", ", result.Errors.Select(e => e.Message));
            logger.LogError("Result Failed: {Error}", error);
            await Clients.All.SendAsync("ChaosError", error);
            return;
        }

        logger.LogInformation("Local chaos running... Good luck!");
    }

    public async Task StartTwitchPollChaos()
    {
        var loadResult = await payloadLoader.LoadPayloadsAsync();

        if (loadResult.IsFailed)
        {
            var error = string.Join(", ", loadResult.Errors.Select(e => e.Message));
            logger.LogError("Payload Load Failed: {Error}", error);
            await Clients.All.SendAsync("ChaosError", error);
            return;
        }

        var result = await twitchConductor.InitializeAsync();

        if (result.IsFailed)
        {
            var error = string.Join(", ", result.Errors.Select(e => e.Message));
            logger.LogError("Result Failed: {Error}", error);
            await Clients.All.SendAsync("ChaosError", error);
            return;
        }

        logger.LogInformation("Twitch poll chaos running... Best of luck to you, my friend!");
    }

    public async Task StartTwitchBot()
    {
        await Clients.All.SendAsync("TwitchStateChanged", UiWidgetState.Progress);
        var result = await twitchBot.ConnectAsync();

        if (result.IsFailed)
        {
            var reasons = string.Join(", ", result.Errors.Select(e => e.Message));
            logger.LogError("Result Failed: {Result}", reasons);
            await Clients.All.SendAsync("TwitchFailed", reasons);
            return;
        }

        logger.LogInformation("Twitch bot started...");
    }

    public void StopTwitchBot() => twitchBot.Stop();

    public void StopAllConductors()
    {
        localConductor.Stop();
        twitchConductor.Stop();
    }

    public void UnstuckMove(string coord, float amount)
    {
        logger.LogInformation("Unstuck request for axis {coord} by {amount}", coord, amount);

        var xMod = coord == "x" ? amount : 0.0;
        var zMod = coord == "z" ? amount : 0.0;

        queue.AddPayload(new()
        {
            Name = "Unstuck Request",
            Sequence = [
                new() {
                    Delay = TimeSpan.Zero,
                    AngelCode = $"SetPlayerPos(GetPlayerPosX() + {xMod}, GetPlayerPosY(), GetPlayerPosZ() + {zMod})"
                }
            ]
        });
    }
}
