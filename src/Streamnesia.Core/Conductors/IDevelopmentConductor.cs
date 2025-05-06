namespace Streamnesia.Core.Conductors;

public interface IDevelopmentConductor
{
    event Func<string, CancellationToken, Task>? OnErrorAsync;

    Task ExecuteCodeAsync(string angelCode, CancellationToken cancellationToken = default);

    string PreprocessCode(string angelCode);
}
