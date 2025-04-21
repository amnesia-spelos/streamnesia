using Streamnesia.Core.Entities;

namespace Streamnesia.Core;

public interface ICommandQueue
{
    public Task StartCommandProcessingAsync(CancellationToken cancellationToken);

    public void AddPayload(PayloadModel model);
}
