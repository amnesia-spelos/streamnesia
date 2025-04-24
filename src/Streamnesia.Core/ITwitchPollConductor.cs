using FluentResults;
using Streamnesia.Core.Entities;

namespace Streamnesia.Core;

public interface ITwitchPollConductor // NOTE(spelos): Maybe both conductors should share IConductor?
{
    event Func<CancellationToken, Task>? PollStartedAsync;

    Task<Result> InitializeAsync();

    Result Stop();

    Result<IReadOnlyCollection<ParsedPayload>> BeginPollRound();

    Result ExecuteTopPayload();

    Result<IReadOnlyDictionary<ParsedPayload, int>> GetCurrentPollState();
}
