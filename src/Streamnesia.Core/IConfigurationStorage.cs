﻿using Streamnesia.Core.Configuration;

namespace Streamnesia.Core;

public interface IConfigurationStorage
{
    AmnesiaClientConfig ReadAmnesiaClientConfig();

    void WriteAmnesiaClientConfig(AmnesiaClientConfig newConfig);

    TwitchBotConfig ReadTwitchBotConfig();

    void WriteTwitchBotConfig(TwitchBotConfig newConfig);

    PayloadLoaderConfig ReadPayloadLoaderConfig();

    void WritePayloadLoaderConfig(PayloadLoaderConfig newConfig);

    LocalChaosConfig ReadLocalChaosConfig();

    void WriteLocalChaosConfig(LocalChaosConfig newConfig);

    TwitchPollConfig ReadTwitchPollConfig();

    void WriteTwitchPollConfig(TwitchPollConfig newConfig);

    DeveloperConfig ReadDeveloperConfig();

    void WriteDeveloperConfig(DeveloperConfig newConfig);
}
