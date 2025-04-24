using System.ComponentModel.DataAnnotations;

namespace Streamnesia.Core.Configuration;

public class TwitchPollConfig
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Vote round length must be at least 1 second.")]
    public int VoteRoundLengthInSeconds { get; set; } = 30;
}
