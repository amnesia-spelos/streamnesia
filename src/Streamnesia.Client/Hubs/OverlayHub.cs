using Microsoft.AspNetCore.SignalR;
using Streamnesia.Core.Conductors;

namespace Streamnesia.Client.Hubs;

public class OverlayHub(
    ITwitchPollConductor conductor,
    ILogger<OverlayHub> logger) : Hub
{
    public async Task StartTwitchPollChaos()
    {
        var result = await conductor.InitializeAsync();

        if (result.IsFailed)
        {
            logger.LogError("Result Failed: {Result}", string.Join(", ", result.Errors.Select(e => e.Message)));
            return;
        }

        logger.LogInformation("Twitch Poll Chaos running...");
    }
}
