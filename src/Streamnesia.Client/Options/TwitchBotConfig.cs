using Streamnesia.Core.Configuration;

namespace Streamnesia.Client.Options;

public class TwitchBotConfig : ITwitchBotConfig
{
    public string BotApiKey { get; set; } = string.Empty;

    public string BotName { get; set; } = string.Empty;

    public string TwitchChannelName { get; set; } = string.Empty;
}
