namespace Streamnesia.Client.Models;

public class IndexViewModel
{
    public UiWidgetState CurrentAmnesiaClientState { get; set; }

    public UiWidgetState CurrentTwitchBotState { get; set; }

    public bool LocalChaosRunning { get; set; }

    public bool TwitchChaosRunning { get; set; }

    public bool PayloadsLoaded { get; set; }

    public bool DeveloperMode { get; set; }
}

public enum UiWidgetState
{
    Disabled,
    Ready,
    Progress,
    Error,
    Success
}
