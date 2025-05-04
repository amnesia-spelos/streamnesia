using FluentResults;

namespace Streamnesia.Core;

public interface ILocalPayloadConductor
{
    public bool IsRunning { get; }

    Result Start();

    Result Stop();
}
