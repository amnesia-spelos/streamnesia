using Streamnesia.Core.Configuration;

namespace Streamnesia.Client.Models;

public class SettingsModel
{
    public required AmnesiaClientConfig AmnesiaClientConfig { get; set; }

    public required TwitchBotConfig TwitchBotConfig { get; set; }
}
