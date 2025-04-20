using System;
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

    private readonly Stopwatch _stopwatch = new();

    private NetworkStream _stream;
    private StreamWriter _writer;
    private StreamReader _reader;

    public bool IsConnected => client.Connected;

    public event AsyncStateChangedHandler StateChangedAsync;

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
                await ((AsyncStateChangedHandler)handler)(this, newState);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to raise state changed event");
            }
        }
    }

    public Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (client.Connected)
        {
            logger.LogWarning("The game client is already connected and won't be re-initialized.");
            return Task.FromResult(Result.Fail("The game client is already connected!"));
        }

        State = AmnesiaClientState.Connecting;

        var config = configStorage.ReadAmnesiaClientConfig();
        client.Connect(config.Host, config.Port); // FIXME: will throw if the game is not up, therefore, this must be retry-able
        _stream = client.GetStream();
        _writer = new StreamWriter(_stream, Encoding.ASCII) { AutoFlush = true };
        _reader = new StreamReader(_stream, Encoding.ASCII);

        _ = Task.Run(() => WaitForWelcomeMessageAsync(cancellationToken), CancellationToken.None);

        return Task.FromResult(Result.Ok());
    }

    private async Task WaitForWelcomeMessageAsync(CancellationToken cancellationToken)
    {
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

        _stopwatch.Restart();

        _writer.Write($"exec:{command}");
        var response = await _reader.ReadLineAsync();

        _stopwatch.Stop();

        logger.LogInformation("Game Response: {response} - (took {ElapsedMilliseconds}ms)", response, _stopwatch.ElapsedMilliseconds);
        return Result.Ok();
    }

    public void Dispose()
    {
        client.Dispose();
        _stream.Dispose();

        GC.SuppressFinalize(this);
    }
}
