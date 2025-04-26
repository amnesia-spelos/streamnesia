using FluentResults;

namespace Streamnesia.Core;

public enum TwitchBotState
{
    Disconnected,
    Connecting,
    Connected,
    Failed
}

public delegate Task AsyncTwitchBotStateChangedHandler(object? sender, TwitchBotState newState, string message);

public record struct MessageEventArgs(string UserId, string Message);

public interface ITwitchBot : IDisposable
{
    public event AsyncTwitchBotStateChangedHandler? StateChangedAsync;

    public TwitchBotState State { get; }

    bool IsConnected { get; }

    Task<Result> ConnectAsync(CancellationToken cancellationToken = default);

    void Stop(bool stateChange = true);

    event EventHandler<MessageEventArgs> MessageReceived;
}
