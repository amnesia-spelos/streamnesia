using FluentResults;
using Streamnesia.Core.Entities;

namespace Streamnesia.Core;

public interface IPayloadLoader
{
    Task<Result> LoadPayloadsAsync(CancellationToken cancellationToken = default);

    IReadOnlyCollection<ParsedPayload> Payloads { get; }
}
