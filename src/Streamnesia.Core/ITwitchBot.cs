using FluentResults;

namespace Streamnesia.Core;

public record struct MessageEventArgs(string UserId, string Message);

public interface ITwitchBot
{
    //bool IsConnected { get; }

    //Task<Result> ConnectAsync(CancellationToken cancellationToken = default);

    //event EventHandler<MessageEventArgs> MessageReceived;
}
