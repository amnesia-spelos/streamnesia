using System;

namespace Streamnesia.Core.Entities;

public class TimedInstruction
{
    public DateTime ExecuteAfterDateTime { get; set; }

    public string Angelcode { get; set; } = string.Empty;
}
