using Microsoft.AspNetCore.SignalR;
using Streamnesia.Core;

namespace Streamnesia.Client.Hubs;

public class TwitchClientEventDispatcher
{
    private readonly IHubContext<StatusHub> _hubContext;
    private readonly ILogger<AmnesiaClientEventDispatcher> _logger;

    public TwitchClientEventDispatcher(
        IHubContext<StatusHub> hubContext,
        ITwitchBot twitchBot,
        ILogger<AmnesiaClientEventDispatcher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;

        twitchBot.StateChangedAsync += OnTwitchBotStateChangedAsync;
    }

    private async Task OnTwitchBotStateChangedAsync(object? sender, TwitchBotState newState, string message)
    {
        try
        {
            if (newState == TwitchBotState.Failed)
            {
                await _hubContext.Clients.All.SendAsync("TwitchFailed", message);
                return;
            }

            var uiState = newState.ToUiState();
            _logger.LogInformation("Broadcasting Twitch bot state: {State}", uiState.ToString());
            await _hubContext.Clients.All.SendAsync("TwitchStateChanged", uiState.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast AmnesiaClient state change.");
        }
    }
}
