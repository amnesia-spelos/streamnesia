using Microsoft.AspNetCore.SignalR;
using Streamnesia.Core;

namespace Streamnesia.Client.Hubs;

public class SettingsHub(IConfigurationStorage cfgStorage, ILogger<SettingsHub> logger) : Hub
{
    private const string EchoMethodName = "Echo";

    public async Task Echo(string message)
    {
        logger.LogInformation("Received message: {message}", message);
        await Clients.All.SendAsync(EchoMethodName, message);
    }
}
