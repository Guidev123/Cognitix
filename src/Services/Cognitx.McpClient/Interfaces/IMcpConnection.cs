using ModelContextProtocolClient = ModelContextProtocol.Client.McpClient;

namespace Cognitx.McpClient.Interfaces
{
    public interface IMcpConnection : IAsyncDisposable
    {
        ModelContextProtocolClient Client { get; }

        Task EnsureConnectedAsync(CancellationToken cancellationToken = default);

        bool IsConnected { get; }
    }
}