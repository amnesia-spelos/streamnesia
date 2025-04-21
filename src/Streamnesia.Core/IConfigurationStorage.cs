using Streamnesia.Core.Configuration;

namespace Streamnesia.Core;

public interface IConfigurationStorage
{
    AmnesiaClientConfig ReadAmnesiaClientConfig();

    void WriteAmnesiaClientConfig(AmnesiaClientConfig newConfig);

    TwitchBotConfig ReadTwitchBotConfig();

    void WriteTwitchBotConfig(TwitchBotConfig newConfig);
}
