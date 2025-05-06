using Streamnesia.Core;
using Streamnesia.Core.Configuration;

namespace Streamnesia.Configuration;

public class ConfigurationStorage(
    IStoredItem<AmnesiaClientConfig> storedAmnesiaClientCfg,
    IStoredItem<TwitchBotConfig> storedTwitchBotCfg,
    IStoredItem<PayloadLoaderConfig> storedPayloadLoaderCfg,
    IStoredItem<LocalChaosConfig> storedLocalChaosCfg,
    IStoredItem<TwitchPollConfig> storedTwitchPollCfg,
    IStoredItem<DeveloperConfig> storedDeveloperCfg
    ) : IConfigurationStorage
{
    public AmnesiaClientConfig ReadAmnesiaClientConfig()
        => ReadOrCreate(storedAmnesiaClientCfg);

    public TwitchBotConfig ReadTwitchBotConfig()
        => ReadOrCreate(storedTwitchBotCfg);

    public PayloadLoaderConfig ReadPayloadLoaderConfig()
        => ReadOrCreate(storedPayloadLoaderCfg);

    public LocalChaosConfig ReadLocalChaosConfig()
        => ReadOrCreate(storedLocalChaosCfg);

    public TwitchPollConfig ReadTwitchPollConfig()
        => ReadOrCreate(storedTwitchPollCfg);

    public DeveloperConfig ReadDeveloperConfig()
        => ReadOrCreate(storedDeveloperCfg);

    public void WriteAmnesiaClientConfig(AmnesiaClientConfig newConfig)
        => storedAmnesiaClientCfg.Overwrite(newConfig);

    public void WriteTwitchBotConfig(TwitchBotConfig newConfig)
        => storedTwitchBotCfg.Overwrite(newConfig);

    public void WritePayloadLoaderConfig(PayloadLoaderConfig newConfig)
    {
        newConfig.CustomPayloadsFile ??= string.Empty;

        storedPayloadLoaderCfg.Overwrite(newConfig);
    }

    public void WriteLocalChaosConfig(LocalChaosConfig newConfig)
        => storedLocalChaosCfg.Overwrite(newConfig);

    public void WriteTwitchPollConfig(TwitchPollConfig newConfig)
        => storedTwitchPollCfg.Overwrite(newConfig);

    public void WriteDeveloperConfig(DeveloperConfig newConfig)
        => storedDeveloperCfg.Overwrite(newConfig);

    private static TConfig ReadOrCreate<TConfig>(IStoredItem<TConfig> storedItem) where TConfig : class, new()
    {
        var cfg = storedItem.Retrieve();

        if (cfg.IsFailed)
        {
            // TODO: propagate better
            throw new InvalidOperationException("storage operation failed");
        }

        if (cfg.Value is not null)
            return cfg.Value;

        var newCfg = new TConfig();
        storedItem.Overwrite(newCfg);

        return newCfg;
    }
}
