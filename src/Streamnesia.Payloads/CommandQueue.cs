using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Streamnesia.Core;
using Streamnesia.Core.Entities;

namespace Streamnesia.Payloads;

public class CommandQueue(IAmnesiaClient amnesiaClient, ICommandPreprocessor commandPreprocessor, ILogger<CommandQueue> logger) : ICommandQueue
{
    private readonly ConcurrentQueue<PayloadModel> _payloadQueue = new();
    private readonly ConcurrentQueue<TimedInstruction> _instructionQueue = new();

    public async Task StartCommandProcessingAsync(CancellationToken cancellationToken)
    {
        if (!amnesiaClient.IsConnected)
        {
            logger.LogError("Cannot start command queue processing because the Amnesia client is not connected");
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            DequeueAndScheduleAvailablePayload();

            await ProcessElapsedPayloadsAsync();

            await Task.Delay(1, cancellationToken);
        }
    }

    public void AddPayload(PayloadModel model)
    {
        if(string.IsNullOrWhiteSpace(model.Name))
        {
            logger.LogError("The CommandQueue received an empty-named payload. This payload will not be executed.");
            logger.LogError("The empty-named payload contains {Length} sequence item(s).", model.Sequence.Length);
            return;
        }
        
        _payloadQueue.Enqueue(model);
    }

    private void DequeueAndScheduleAvailablePayload()
    {
        if (!amnesiaClient.IsConnected || _payloadQueue.IsEmpty)
            return;

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

    private async Task ProcessElapsedPayloadsAsync()
    {
        for (var i = 0; i < _instructionQueue.Count; i++)
        {
            if (_instructionQueue.TryDequeue(out TimedInstruction extension) == false)
            {
                logger.LogError("Failed to dequeue an instruction");
                return;
            }

            if (DateTime.Now >= extension.ExecuteAfterDateTime)
            {
                logger.LogInformation("Sending code: {Angelcode}", extension.Angelcode);
                await amnesiaClient.ExecuteCommandAsync(extension.Angelcode);
                return;
            }

            _instructionQueue.Enqueue(extension);
        }
    }
}
