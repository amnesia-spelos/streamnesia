﻿using FluentResults;

namespace Streamnesia.Core;

public enum AmnesiaClientState
{
    Disconnected,
    Connecting,
    Connected,
    Failed
}

public delegate Task AsyncAmnesiaStateChangedHandler(object? sender, AmnesiaClientState newState, string message);

public interface IAmnesiaClient : IDisposable
{
    public event AsyncAmnesiaStateChangedHandler? StateChangedAsync;

    public AmnesiaClientState State { get; }

    public bool IsConnected { get; }

    public Task<Result> ConnectAsync(CancellationToken cancellationToken = default);

    public Task<Result> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);

    public Task Disconnect(CancellationToken cancellationToken = default);
}
