using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Streamnesia.Core;
using Streamnesia.Core.Entities;

namespace Streamnesia.Payloads;

public class CommandQueue(
    IAmnesiaClient amnesiaClient,
    ICommandPreprocessor commandPreprocessor,
    ILogger<CommandQueue> logger) : ICommandQueue
{
    private readonly ConcurrentQueue<PayloadModel> _payloadQueue = new();
    private readonly ConcurrentQueue<TimedInstruction> _instructionQueue = new();

    private CancellationTokenSource _cts;
    private Task _processingTask;
    private readonly object _lock = new();

    public void AddPayload(PayloadModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            logger.LogError("The CommandQueue received an empty-named payload. This payload will not be executed.");
            logger.LogError("The empty-named payload contains {Length} sequence item(s).", model.Sequence.Length);
            return;
        }

        logger.LogInformation("Enqueue: {Name}", model.Name);
        _payloadQueue.Enqueue(model);
    }

    public void Start()
    {
        lock (_lock)
        {
            if (_processingTask != null && !_processingTask.IsCompleted)
            {
                logger.LogWarning("CommandQueue already running.");
                return;
            }

            _cts = new CancellationTokenSource();
            _processingTask = Task.Run(() => RunLoopAsync(_cts.Token));
        }
    }

    public async Task StopAsync()
    {
        lock (_lock)
        {
            if (_cts == null)
                return;

            _cts.Cancel();
        }

        if (_processingTask != null)
        {
            await _processingTask;
        }

        _cts?.Dispose();
        _cts = null;
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        if (!amnesiaClient.IsConnected)
        {
            logger.LogError("Amnesia client is not connected. CommandQueue will not run.");
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1));

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                DequeueAndScheduleAvailablePayload();
                await ProcessElapsedPayloadsAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("CommandQueue cancelled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in CommandQueue loop.");
        }
    }

    private void DequeueAndScheduleAvailablePayload()
    {
        if (!amnesiaClient.IsConnected)
        {
            logger.LogWarning("Refusing to dequeue a payload because the game client is not connected.");
            return;
        }

        if (_payloadQueue.IsEmpty)
        {
            logger.LogDebug("Nothing to dequeue");
            return;
        }

        if(_payloadQueue.TryDequeue(out var model))
        {
            ProcessPayload(model);
        }
        else
        {
            logger.LogError("A dequeue of a payload failed");
        }
    }

    private void ProcessPayload(PayloadModel model)
    {
        logger.LogDebug("processing payload: {Name}", model.Name);
        foreach (var sequenceItem in model.Sequence)
        {
            try
            {
                var code = commandPreprocessor.PreprocessCommand(File.ReadAllText(sequenceItem.File));
                _instructionQueue.Enqueue(new TimedInstruction
                {
                    Angelcode = code,
                    ExecuteAfterDateTime = DateTime.Now.Add(sequenceItem.Delay)
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "An exception occurred while trying to execute a timed instruction");
            }
        }
    }

    private async Task ProcessElapsedPayloadsAsync(CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Processing queue count of {Count}", _instructionQueue.Count);
        for (var i = 0; i < _instructionQueue.Count; i++)
        {
            logger.LogDebug("Iteration {i}", i);
            if (_instructionQueue.TryDequeue(out TimedInstruction extension) == false)
            {
                logger.LogError("Failed to dequeue an instruction");
                return;
            }

            if (DateTime.Now >= extension.ExecuteAfterDateTime)
            {
                logger.LogInformation("Sending code: {Angelcode}", extension.Angelcode);
                await amnesiaClient.ExecuteCommandAsync(extension.Angelcode, CancellationToken.None);
                return;
            }

            logger.LogDebug("DateTime {Now} is not after {ExecuteAfterDateTime}", DateTime.Now, extension.ExecuteAfterDateTime);
            _instructionQueue.Enqueue(extension);
        }
    }
}
