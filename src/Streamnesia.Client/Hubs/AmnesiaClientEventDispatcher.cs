using Microsoft.AspNetCore.SignalR;
using Streamnesia.Core;

namespace Streamnesia.Client.Hubs;

public class AmnesiaClientEventDispatcher
{
    private readonly IHubContext<StatusHub> _hubContext;
    private readonly ILogger<AmnesiaClientEventDispatcher> _logger;

    public AmnesiaClientEventDispatcher(IAmnesiaClient amnesiaClient, IHubContext<StatusHub> hubContext, ILogger<AmnesiaClientEventDispatcher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;

        amnesiaClient.StateChangedAsync += OnAmnesiaClientStateChangedAsync;
    }

    private async Task OnAmnesiaClientStateChangedAsync(object? sender, AmnesiaClientState state, string message)
    {
        try
        {
            _logger.LogInformation("Broadcasting AmnesiaClient state: {State}", state);
            await _hubContext.Clients.All.SendAsync("AmnesiaClientStateChanged", state.ToString(), message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast AmnesiaClient state change.");
        }
    }
}
