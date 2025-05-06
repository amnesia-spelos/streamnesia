namespace Streamnesia.Core.Conductors;

public interface IDevelopmentConductor
{
    event Func<string, CancellationToken, Task>? OnErrorAsync;

    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
