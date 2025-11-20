using System.Net.Sockets;
using FluentResults;

namespace Streamnesia.Core.Amnesia;

public class AmnesiaClient(ITcpClient tcpClient) : IAmnesiaClient
{
    public AmnesiaClientState State => throw new NotImplementedException();

    public bool IsConnected => tcpClient.Connected;

    public event AsyncAmnesiaStateChangedHandler? StateChangedAsync;

    public Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Disconnect(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        tcpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task<Result> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
