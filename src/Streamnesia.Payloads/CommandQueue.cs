using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Streamnesia.CommandProcessing.Entities;
using Streamnesia.Payloads;

namespace Streamnesia.Payloads;

public class CommandQueue
{
    private ConcurrentQueue<Payload> _payloadQueue = new ConcurrentQueue<Payload>();
    private ConcurrentQueue<TimedInstruction> _instructionQueue = new ConcurrentQueue<TimedInstruction>();
    public AmnesiaClient Amnesia { get; private set; } = new(new Random());
    private bool _initialized = false;

    public async Task StartCommandProcessingAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Amnesia.AttachToGameAsync();
            _initialized = true;
        }
        catch (Exception)
        {
            Console.WriteLine("ERROR: Failed to attach to game");
            throw;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            ProcessCommandQueue();
            await ProcessInstructionQueue();
            await Task.Delay(1, cancellationToken);
        }

        Amnesia.Dispose();
    }

    public void AddPayload(Payload payload)
    {
        if(string.IsNullOrWhiteSpace(payload.Name))
        {
            Console.WriteLine("The CommandQueue received an empty-named payload. This payload will not be executed.");
            Console.WriteLine($"Thie empty-named payload contains {payload.Sequence.Length} sequence item(s).");
            return;
        }
        
        _payloadQueue.Enqueue(payload);
    }

    private void ProcessCommandQueue()
    {
        if (!_initialized || _payloadQueue.Count == 0)
            return;

        if(_payloadQueue.TryDequeue(out var payload) == false)
        {
            Console.WriteLine("A dequeue of a payload failed");
            return;
        }

        ProcessPayload(payload);
    }

    private async Task ProcessInstructionQueue()
    {
        TimedInstruction extension;

        for(var i = 0; i < _instructionQueue.Count; i++)
        {
            if(_instructionQueue.TryDequeue(out extension) == false)
            {
                Console.WriteLine("Failed to dequeue an instruction");
                return;
            }

            if(DateTime.Now >= extension.ExecuteAfterDateTime)
            {
                Console.WriteLine($"Sending code: {extension.Angelcode}");
                await Amnesia.ExecuteAsync(extension.Angelcode);
                return;
            }

            _instructionQueue.Enqueue(extension);
        }
    }

    private void ProcessPayload(Payload payload)
    {
        foreach (var sequenceItem in payload.Sequence)
        {
            _instructionQueue.Enqueue(new TimedInstruction
            {
                Angelcode = sequenceItem.AngelCode,
                ExecuteAfterDateTime = DateTime.Now.Add(sequenceItem.Delay)
            });
        }
    }
}
