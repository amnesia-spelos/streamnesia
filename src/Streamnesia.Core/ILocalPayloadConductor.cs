using FluentResults;

namespace Streamnesia.Core;

public interface ILocalPayloadConductor
{
    Result Start();

    Result Stop();
}
