using System.ComponentModel.DataAnnotations;

namespace Streamnesia.Core.Configuration;

public class TwitchBotConfig
{
    [Required]
    public string BotApiKey { get; set; } = string.Empty;

    [Required]
    public string BotName { get; set; } = string.Empty;

    [Required]
    public string TwitchChannelName { get; set; } = string.Empty;
}
