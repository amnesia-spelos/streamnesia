using FluentResults;

namespace Streamnesia.Core;

public interface IAmnesiaClient : IDisposable
{
    public bool IsConnected { get; }

    public Task<Result> ConnectAsync(CancellationToken cancellationToken = default);

    public Task<Result> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);
}
