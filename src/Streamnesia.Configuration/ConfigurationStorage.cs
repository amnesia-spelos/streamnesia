using Streamnesia.Core;
using Streamnesia.Core.Configuration;

namespace Streamnesia.Configuration;

public class ConfigurationStorage(
    IStoredItem<AmnesiaClientConfig> storedAmnesiaClientCfg,
    IStoredItem<TwitchBotConfig> storedTwitchBotCfg,
    IStoredItem<PayloadLoaderConfig> storedPayloadLoaderCfg,
    IStoredItem<LocalChaosConfig> storedLocalChaosCfg
    ) : IConfigurationStorage
{
    public AmnesiaClientConfig ReadAmnesiaClientConfig()
    {
        var cfg = storedAmnesiaClientCfg.Retrieve();

        if (cfg.IsFailed)
        {
            // TODO: propagate better
            throw new InvalidOperationException("storage operation failed");
        }

        if (cfg.Value is not null)
            return cfg.Value;

        var newCfg = new AmnesiaClientConfig();
        storedAmnesiaClientCfg.Overwrite(newCfg);

        return newCfg;
    }

    public TwitchBotConfig ReadTwitchBotConfig()
    {
        var cfg = storedTwitchBotCfg.Retrieve();

        if (cfg.IsFailed)
        {
            // TODO: propagate better
            throw new InvalidOperationException("storage operation failed");
        }

        if (cfg.Value is not null)
            return cfg.Value;

        var newCfg = new TwitchBotConfig();
        storedTwitchBotCfg.Overwrite(newCfg);

        return newCfg;
    }

    public PayloadLoaderConfig ReadPayloadLoaderConfig()
    {
        var cfg = storedPayloadLoaderCfg.Retrieve();

        if (cfg.IsFailed)
        {
            // TODO: propagate better
            throw new InvalidOperationException("storage operation failed");
        }

        if (cfg.Value is not null)
            return cfg.Value;

        var newCfg = new PayloadLoaderConfig();
        storedPayloadLoaderCfg.Overwrite(newCfg);

        return newCfg;
    }

    public LocalChaosConfig ReadLocalChaosConfig()
    {
        var cfg = storedLocalChaosCfg.Retrieve();

        if (cfg.IsFailed)
        {
            // TODO: propagate better
            throw new InvalidOperationException("storage operation failed");
        }

        if (cfg.Value is not null)
            return cfg.Value;

        var newCfg = new LocalChaosConfig();
        storedLocalChaosCfg.Overwrite(newCfg);

        return newCfg;
    }

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
}
