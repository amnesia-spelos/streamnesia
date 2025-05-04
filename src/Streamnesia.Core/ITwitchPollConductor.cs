using FluentResults;
using Streamnesia.Core.Entities;

namespace Streamnesia.Core;

public interface ITwitchPollConductor // NOTE(spelos): Maybe both conductors should share IConductor?
{
    bool IsRunning { get; }

    event Func<CancellationToken, Task>? PollStartedAsync;

    Task<Result> InitializeAsync();

    Result Stop();

    Result<IReadOnlyCollection<ParsedPayload>> BeginPollRound();

    Result ExecuteTopPayload();

    Result<IReadOnlyDictionary<ParsedPayload, int>> GetCurrentPollState();
}
