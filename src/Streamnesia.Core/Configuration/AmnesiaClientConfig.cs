using System.ComponentModel.DataAnnotations;

namespace Streamnesia.Core.Configuration;

public class AmnesiaClientConfig
{
    [Required]
    public string Host { get; set; } = "127.0.0.1";

    [Required]
    public int Port { get; set; } = 5150;
}
