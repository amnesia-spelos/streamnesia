namespace Streamnesia.Core;

public interface ITcpClient : IDisposable
{
    public bool Connected { get; }
}
