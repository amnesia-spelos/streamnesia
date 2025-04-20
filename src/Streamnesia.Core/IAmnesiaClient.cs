using FluentResults;

namespace Streamnesia.Core;

public enum AmnesiaClientState
{
    Disconnected,
    Connecting,
    Connected,
    Failed
}

public delegate Task AsyncStateChangedHandler(object? sender, AmnesiaClientState newState);

public interface IAmnesiaClient : IDisposable
{
    public event AsyncStateChangedHandler? StateChangedAsync;

    public AmnesiaClientState State { get; }

    public bool IsConnected { get; }

    public Task<Result> ConnectAsync(CancellationToken cancellationToken = default);

    public Task<Result> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);
}
