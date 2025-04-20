namespace Streamnesia.Core.Configuration;

public interface ITwitchBotConfig
{
    string BotApiKey { get; }

    string BotName { get; }

    string TwitchChannelName { get; }
}
