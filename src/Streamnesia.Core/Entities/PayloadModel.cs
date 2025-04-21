namespace Streamnesia.Core.Entities;

public class PayloadModel
{
    public string Name { get; set; } = string.Empty;

    public PayloadSequenceModel[] Sequence { get; set; } = [];
}

public class PayloadSequenceModel
{
    public string File { get; set; } = string.Empty;

    public TimeSpan Delay { get; set; }
}
