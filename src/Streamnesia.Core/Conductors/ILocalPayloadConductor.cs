using FluentResults;

namespace Streamnesia.Core.Conductors;

public interface ILocalPayloadConductor
{
    public bool IsRunning { get; }

    Result Start();

    Result Stop();
}
