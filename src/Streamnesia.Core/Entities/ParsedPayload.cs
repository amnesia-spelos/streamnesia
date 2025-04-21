namespace Streamnesia.Core.Entities;

public class ParsedPayload
{
    public string Name { get; set; } = string.Empty;

    public ParsedPayloadSequenceItem[] Sequence { get; set; } = [];
}

public class ParsedPayloadSequenceItem
{
    public TimeSpan Delay { get; set; }

    public string AngelCode { get; set; } = string.Empty;
}
