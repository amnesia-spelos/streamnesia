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
    ILogger<DevelopmentConductor> logger
    ) : IDevelopmentConductor
{
    public event Func<string, CancellationToken, Task>? OnErrorAsync;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Development conductor executed");
        
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

        logger.LogInformation("Development conductor completed");
    }
}
