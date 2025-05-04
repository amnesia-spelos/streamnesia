using Streamnesia.Client.Models;
using Streamnesia.Core;

namespace Streamnesia.Client;

public static class UiUtilities
{
    public static UiWidgetState ToUiState(this AmnesiaClientState amnesiaClientState) =>
    amnesiaClientState switch
    {
        AmnesiaClientState.Connected => UiWidgetState.Success,
        AmnesiaClientState.Connecting => UiWidgetState.Progress,
        AmnesiaClientState.Failed => UiWidgetState.Error,
        AmnesiaClientState.Disconnected => UiWidgetState.Ready,
        _ => throw new NotImplementedException()
    };

    public static UiWidgetState ToUiState(this TwitchBotState twitchBotState) =>
        twitchBotState switch
        {
            TwitchBotState.Connected => UiWidgetState.Success,
            TwitchBotState.Connecting => UiWidgetState.Progress,
            TwitchBotState.Failed => UiWidgetState.Error,
            TwitchBotState.Disconnected => UiWidgetState.Ready,
            _ => throw new NotImplementedException()
        };
}
