using Streamnesia.Core.Configuration;

namespace Streamnesia.Client.Options;

public class AmnesiaClientConfig : IAmnesiaClientConfig
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }
}
