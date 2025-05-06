using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Streamnesia.Core;
using Streamnesia.Core.Conductors;

namespace Streamnesia.Execution.Conductors;

internal class DevelopmentConductor(
    IAmnesiaClient amnesiaClient,
    ICommandQueue commandQueue,
    ICommandPreprocessor preprocessor,
    ILogger<DevelopmentConductor> logger
    ) : IDevelopmentConductor
{
    public event Func<string, CancellationToken, Task>? OnErrorAsync;

    private bool _commandQueueInitialized;

    public async Task ExecuteCodeAsync(string angelCode, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Development conductor executed");
        
        if (!amnesiaClient.IsConnected)
        {
            logger.LogDebug("Amnesia client not connected, attempting to connect...");
            var result = await amnesiaClient.ConnectAsync(cancellationToken);

            if (result.IsFailed)
            {
                var reason = string.Join(", ", result.Errors.Select(e => e.Message));
                
                if (OnErrorAsync is not null)
                    await OnErrorAsync.Invoke(reason, cancellationToken);
            }
        }

        if (!_commandQueueInitialized)
        {
            logger.LogInformation("Initializing command queue");
            commandQueue.Start();

            _commandQueueInitialized = true;
        }

        commandQueue.AddPayload(new()
        {
            Name = "Test Payload",
            Sequence = [
                new()
                {
                    Delay = TimeSpan.FromSeconds(0),
                    AngelCode = angelCode
                }]
        });

        logger.LogDebug("Development conductor completed");
    }

    public string PreprocessCode(string angelCode)
        => preprocessor.PreprocessCommand(angelCode);
}
