using Microsoft.AspNetCore.SignalR;
using Streamnesia.Core;

namespace Streamnesia.Client.Hubs;

// TODO: NOTE(spelos): we're doing this kind of loop in a number of places
//                     maybe we should standardize it and hide it?
public class TwitchPollDispatcher
{
    private readonly IHubContext<OverlayHub> hubContext;
    private readonly ITwitchPollConductor conductor;
    private readonly ILogger<TwitchPollDispatcher> logger;
    private readonly IConfigurationStorage cfgStorage;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    public TwitchPollDispatcher(
        IHubContext<OverlayHub> hubContext,
        ITwitchPollConductor conductor,
        IConfigurationStorage cfgStorage,
        ILogger<TwitchPollDispatcher> logger)
    {
        this.hubContext = hubContext;
        this.conductor = conductor;
        this.logger = logger;
        this.cfgStorage = cfgStorage;

        conductor.PollStartedAsync += OnPollStartedAsync;
        logger.LogInformation("Dispatcher attached to conductor");
    }

    private async Task OnPollStartedAsync(CancellationToken token)
    {
        // Cancel any previous poll loop
        await StopAsync();

        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        _loopTask = RunLoopAsync(_cts.Token);

        logger.LogInformation("Poll update loop started by event");
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        var cfg = cfgStorage.ReadTwitchPollConfig();

        if (cfg is null)
        {
            logger.LogWarning("Configuration is null");
            return;
        }

        var voteRoundLength = cfg.VoteRoundLengthInSeconds;
        var elapsedSeconds = int.MaxValue; // Starting expired to initialize new poll
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                logger.LogDebug("TwitchPollDispatcher ticked - update pending");

                if (elapsedSeconds >= voteRoundLength)
                {
                    if (elapsedSeconds != int.MaxValue)
                    {
                        logger.LogInformation("Poll expired, executing top payload");
                        var result = conductor.ExecuteTopPayload();

                        if (result.IsFailed)
                        {
                            logger.LogError("Voted payload execution failed: {Result}", string.Join(", ", result.Errors.Select(e => e.Message)));
                            return;
                        }
                    }

                    elapsedSeconds = 0;

                    var payloadsResult = conductor.BeginPollRound();

                    if (payloadsResult.IsFailed)
                    {
                        logger.LogError("Stating a new poll failed: {Result}", string.Join(", ", payloadsResult.Errors.Select(e => e.Message)));
                        return;
                    }

                    logger.LogInformation("New poll round began...");
                }
                else
                {
                    elapsedSeconds++;
                    logger.LogDebug("UI tick - elapsed seconds: {ElapsedSeconds}", elapsedSeconds);
                }

                var pollStateResult = conductor.GetCurrentPollState();

                if (pollStateResult.IsFailed)
                {
                    logger.LogError("Failed to get poll state: {Result}", string.Join(", ", pollStateResult.Errors.Select(e => e.Message)));
                    continue;
                }

                var percentage = Math.Round((double)elapsedSeconds / voteRoundLength * 100);

                 await hubContext.Clients.All.SendCoreAsync("UpdateTimePercentage", [ new {
                        percentage,
                        options = pollStateResult.Value.Select((kv, index) => new {
                            name = kv.Key.Name,
                            votes = kv.Value,
                            index
                        }),
                        rapidFire = false // NOTE(spelos): Deprecated feature, might be added later
                    }
                 ], cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in TwitchPollDispatcher loop");
            }
        }
    }

    private async Task StopAsync()
    {
        if (_cts is not null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        if (_loopTask is not null)
        {
            try
            {
                await _loopTask;
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Poll loop canceled");
            }
        }
    }
}
