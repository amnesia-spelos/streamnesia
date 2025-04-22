using Microsoft.AspNetCore.SignalR;
using Streamnesia.Core;

namespace Streamnesia.Client.Hubs;

public class StatusHub(IAmnesiaClient amnesiaClient) : Hub
{
    public Task StartAmnesiaClient() => amnesiaClient.ConnectAsync();

    public Task StopAmnesiaClient() => amnesiaClient.Disconnect();
}
