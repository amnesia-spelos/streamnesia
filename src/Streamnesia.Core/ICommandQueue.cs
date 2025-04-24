using Streamnesia.Core.Entities;

namespace Streamnesia.Core;

public interface ICommandQueue
{
    public void AddPayload(ParsedPayload model);

    public void Start();

    public Task StopAsync();
}
