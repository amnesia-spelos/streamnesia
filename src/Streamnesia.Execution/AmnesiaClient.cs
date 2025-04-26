using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using Streamnesia.Core;

namespace Streamnesia.Execution;

public partial class AmnesiaClient(
    TcpClient client,
    IConfigurationStorage configStorage,
    ILogger<AmnesiaClient> logger
    ) : IAmnesiaClient
{
    public bool IsConnected => client.Connected;

    public event AsyncAmnesiaStateChangedHandler? StateChangedAsync;

    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingResponses = new();
    private readonly Stopwatch _stopwatch = new();

    private NetworkStream? _stream;
    private StreamWriter? _writer;
    private StreamReader? _reader;
    private CancellationTokenSource? _listenCts;

    // FIXME: There is definitely a better way to return errors to the UI
    private string _errorMessage = string.Empty;

    private AmnesiaClientState _state;
    public AmnesiaClientState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                _state = value;
                _ = RaiseStateChangedAsync(_state);
            }
        }
    }

    private async Task RaiseStateChangedAsync(AmnesiaClientState newState)
    {
        if (StateChangedAsync == null) return;

        foreach (var handler in StateChangedAsync.GetInvocationList())
        {
            try
            {
                await ((AsyncAmnesiaStateChangedHandler)handler)(this, newState, _errorMessage);
                _errorMessage = string.Empty;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to raise state changed event");
            }
        }
    }

    private async Task ListenLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                var line = await ReadNextLineSafe(cancellationToken);
                if (line == null) break; // disconnected

                if (line.StartsWith("EVENT:"))
                {
                    logger.LogInformation("[Game Event] {event}", line.Substring(6));
                }
                else if (line.StartsWith("RESPONSE:"))
                {
                    logger.LogDebug("Amnesia response received: {line}", line);
                    var parts = line.Split(':', 3); // RESPONSE:getmap:level01.map
                    if (parts.Length >= 3)
                    {
                        if (_pendingResponses.TryRemove(parts[1], out var tcs))
                        {
                            tcs.TrySetResult(parts[2]);
                            logger.LogDebug("Pending response removed. Current pending responses count: {Count}", _pendingResponses.Count);
                        }
                        else
                        {
                            logger.LogDebug("Failed to remove a response from the queue");
                        }
                    }
                    else
                    {
                        logger.LogWarning("Unmatched response: {line}", line);
                    }
                }
                else if (line.StartsWith("WARNING:"))
                {
                    logger.LogWarning("[Game Warning] {Warning}", line);
                }
                else
                {
                    logger.LogWarning("Unknown message format: {line}", line);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in socket listen loop.");
        }
    }

    private async Task<string?> ReadNextLineSafe(CancellationToken cancellationToken)
    {
        if (_reader is null)
            return null;

        try
        {
            var line = await _reader.ReadLineAsync(cancellationToken);
            return line;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to read the next client line.");

            return null;
        }
    }

    public async Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (client.Connected)
        {
            logger.LogWarning("The game client is already connected and won't be re-initialized.");
            return Result.Fail("The game client is already connected!");
        }

        State = AmnesiaClientState.Connecting;

        var config = configStorage.ReadAmnesiaClientConfig();

        try
        {
            client.Connect(config.Host, config.Port); // FIXME: will throw if the game is not up, therefore, this must be retry-able
            _stream = client.GetStream();
            _writer = new StreamWriter(_stream, Encoding.ASCII) { AutoFlush = true };
            _reader = new StreamReader(_stream, Encoding.ASCII);
        }
        catch (SocketException e)
        {
            logger.LogError(e, "Failed to start up TCP client");
            _errorMessage = "Failed to connect to the game. Is it running?";
            State = AmnesiaClientState.Failed;
            return Result.Fail(_errorMessage);
        }
        await WaitForWelcomeMessageAsync(cancellationToken);

        _listenCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = Task.Run(() => ListenLoopAsync(cancellationToken), _listenCts.Token);

        return Result.Ok();
    }

    private async Task WaitForWelcomeMessageAsync(CancellationToken cancellationToken)
    {
        if (_reader is null)
        {
            logger.LogError("Failed to read welcome message: _reader was null");
            return;
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromMinutes(5));

        try
        {
            logger.LogInformation("Waiting for the game to respond...");
            var welcome = await _reader.ReadLineAsync(cts.Token);

            if (welcome != null && welcome.StartsWith("Hello, from Amnesia: The Dark Descent!"))
            {
                State = AmnesiaClientState.Connected;
                logger.LogInformation("Game communication established.");
            }
            else
            {
                State = AmnesiaClientState.Failed;
                logger.LogError("Unexpected welcome message: {Message}", welcome);
            }
        }
        catch (OperationCanceledException)
        {
            State = AmnesiaClientState.Failed;
            logger.LogError("Connection timed out waiting for welcome message.");
        }
        catch (Exception ex)
        {
            State = AmnesiaClientState.Failed;
            logger.LogError(ex, "Failed to receive welcome message.");
        }
    }

    public async Task<Result> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Sending command to execute: '{command}'", command);
        return await ExecuteRawAsync($"exec:{command}", cancellationToken);
    }

    public async Task<Result> ExecuteRawAsync(string rawInstruction, CancellationToken cancellationToken = default)
    {
        if (_writer is null)
        {
            logger.LogError("Failed to execute raw command: _writer was null");
            return Result.Fail("Could not execute a command due to the writer being null");
        }

        _stopwatch.Restart();

        string tag = rawInstruction.Split(':')[0]; // e.g., "getmap" or "exec"
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (_pendingResponses.ContainsKey(tag))
        {
            logger.LogDebug("A pending response with the tag '{Tag}' already exists.", tag);
        }

        _pendingResponses[tag] = tcs;

        await _writer.WriteLineAsync(rawInstruction);

        try
        {
            var response = await tcs.Task.WaitAsync(cancellationToken);
            _stopwatch.Stop();

            logger.LogInformation("Game Response: {response} (in {ElapsedMilliseconds}ms)", response, _stopwatch.ElapsedMilliseconds);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Timeout or error waiting for response to {tag}", tag);
            return Result.Fail($"Timeout/error waiting for response: {tag}");
        }
        finally
        {
            _pendingResponses.TryRemove(tag, out _);
        }
    }

    public void Dispose()
    {
        client.Dispose();
        _stream?.Dispose();

        GC.SuppressFinalize(this);
    }

    public Task Disconnect(CancellationToken cancellationToken = default)
    {
        if (client.Connected)
        {
            logger.LogInformation("Disconnecting client...");
            _listenCts?.Cancel(); // cancel the listener before closing the stream
            _listenCts?.Dispose();
            _listenCts = null;

            client.Close();
            client = new TcpClient(); // replace with new instance
            State = AmnesiaClientState.Disconnected;
        }

        return Task.CompletedTask;
    }
}
