using System.ComponentModel.DataAnnotations;

namespace Streamnesia.Core.Configuration;

public class TwitchBotConfig
{
    public string? BotApiKey { get; set; } = string.Empty;

    public string? BotName { get; set; } = string.Empty;

    public string? TwitchChannelName { get; set; } = string.Empty;
}
