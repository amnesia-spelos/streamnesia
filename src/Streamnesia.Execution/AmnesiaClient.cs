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

    public async Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (client.Connected)
        {
            logger.LogWarning("The game client is already connected and won't be re-initialized.");
            return Result.Fail("The game client is already connected!");
        }

        var config = configStorage.ReadAmnesiaClientConfig();

        client.Connect(config.Host, config.Port);
        _stream = client.GetStream();
        _writer = new StreamWriter(_stream, Encoding.ASCII) { AutoFlush = true };
        _reader = new StreamReader(_stream, Encoding.ASCII);

        logger.LogInformation("Waiting for the game to respond...");
        string welcome = await _reader.ReadLineAsync(); // TODO: Cancellation token + retry policy?

        if (!welcome.StartsWith("Hello, from Amnesia: The Dark Descent!"))
        {
            logger.LogError("The game server returned an unexpected welcome message: {Message}", welcome);
            return Result.Fail($"Unexpected welcome message: '{welcome}'");
        }

        logger.LogInformation("Game communication established.");
        return Result.Ok();
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
