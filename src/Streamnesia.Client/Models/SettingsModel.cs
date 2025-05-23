﻿using Streamnesia.Core.Configuration;

namespace Streamnesia.Client.Models;

public class SettingsModel
{
    public required AmnesiaClientConfig AmnesiaClientConfig { get; set; }

    public required TwitchBotConfig TwitchBotConfig { get; set; }

    public required PayloadLoaderConfig PayloadLoaderConfig { get; set; }

    public required LocalChaosConfig LocalChaosConfig { get; set; }

    public required TwitchPollConfig TwitchPollConfig { get; set; }

    public required DeveloperConfig DeveloperConfig { get; set; }
}
