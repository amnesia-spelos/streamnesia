using Microsoft.AspNetCore.SignalR;
using Streamnesia.Core.Conductors;

namespace Streamnesia.Client.Hubs;

public class DevHub(
    IDevelopmentConductor conductor,
    ILogger<DevHub> logger
    ) : Hub
{
    public async Task ExecuteScript(string script)
    {
        logger.LogDebug("Execute Script: {Script}", script);

        await conductor.ExecuteCodeAsync(script);
    }

    public async Task PreprocessScript(string script)
    {
        logger.LogDebug("Preprocess Script: {Script}", script);

        var result = conductor.PreprocessCode(script);

        await Clients.All.SendAsync("ScriptPreprocessed", result);
    }
}
