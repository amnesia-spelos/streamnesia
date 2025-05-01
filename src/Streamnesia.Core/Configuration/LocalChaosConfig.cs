using System.ComponentModel.DataAnnotations;

namespace Streamnesia.Core.Configuration;

public class LocalChaosConfig
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Interval must be at least 1 second long.")]
    public int IntervalInSeconds { get; set; } = 10;

    [Required]
    public bool IsSequential { get; set; } = false;
}
