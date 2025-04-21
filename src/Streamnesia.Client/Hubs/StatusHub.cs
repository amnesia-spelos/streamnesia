using Microsoft.AspNetCore.SignalR;
using Streamnesia.Core;

namespace Streamnesia.Client.Hubs;

public class StatusHub : Hub
{
    private readonly IAmnesiaClient _amnesiaClient;

    public StatusHub(IAmnesiaClient amnesiaClient)
    {
        _amnesiaClient = amnesiaClient;
    }

    public Task StartAmnesiaClient() => _amnesiaClient.ConnectAsync();

    public Task StopAmnesiaClient() => _amnesiaClient.Disconnect();
}
