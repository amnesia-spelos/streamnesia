using System.ComponentModel.DataAnnotations;

namespace Streamnesia.Core.Configuration;

public class PayloadLoaderConfig
{
    [Required]
    public bool DownloadEnabled { get; set; } = true;

    [Required]
    public bool UseVanillaPayloads { get; set; } = true;

    public string? CustomPayloadsFile { get; set; } = string.Empty;
}
