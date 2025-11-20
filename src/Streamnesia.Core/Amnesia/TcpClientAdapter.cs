using System.Net.Sockets;

namespace Streamnesia.Core.Amnesia;

public class TcpClientAdapter(TcpClient client) : ITcpClient
{
    public bool Connected => client.Connected;

    public void Dispose()
    {
        client.Dispose();
        GC.SuppressFinalize(this);
    }
}
