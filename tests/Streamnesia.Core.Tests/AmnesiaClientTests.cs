using Moq;
using Streamnesia.Core.Amnesia;

namespace Streamnesia.Core.Tests;

public class AmnesiaClientTests
{
    private readonly Mock<ITcpClient> _tcpClientMock = new();
    private readonly AmnesiaClient _client;

    public AmnesiaClientTests()
    {
        _client = new(_tcpClientMock.Object);
    }

    [Fact]
    public void Dispose_ShouldDisposeTcpClient()
    {
        _client.Dispose();

        _tcpClientMock.Verify(c => c.Dispose(), Times.Once);
    }

    // TODO: Let's not expose Connected,
    //       let's use the "State" property for that kind of a check
    //       and ensure the value is up to date.
    [Fact]
    public void Client_ShouldProxyConnectedStatus()
    {
        _tcpClientMock.Setup(c => c.Connected).Returns(true);

        Assert.True(_client.IsConnected);
    }
}
