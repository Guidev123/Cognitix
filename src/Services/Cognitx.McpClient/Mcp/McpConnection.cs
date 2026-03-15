using Cognitx.McpClient.Interfaces;
using Cognitx.McpClient.Options;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocolClient = ModelContextProtocol.Client.McpClient;

namespace Cognitx.McpClient.Mcp
{
    public sealed class McpConnection(
        HttpClient httpClient,
        IOptions<McpOptions> options,
        ILogger<McpConnection> logger
        ) : IMcpConnection
    {
        private readonly McpOptions mcpOptions = options.Value;
        private ModelContextProtocolClient? _mcpClient;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private bool _disposed;

        public ModelContextProtocolClient Client => _mcpClient
            ?? throw new InvalidOperationException("Connection not established. Call EnsureConnectedAsync first.");

        public bool IsConnected => _mcpClient is not null;

        public async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
        {
            if (_mcpClient is not null) return;

            await _connectionLock.WaitAsync(cancellationToken);

            try
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Connection to MCP Server at {BaseUrl}", mcpOptions.BaseUrl);
                }

                var transport = new HttpClientTransport(new HttpClientTransportOptions()
                {
                    Endpoint = new Uri(mcpOptions.BaseUrl),
                    Name = "McpClient"
                }, httpClient);

                _mcpClient = await ModelContextProtocolClient.CreateAsync(transport, cancellationToken: cancellationToken);

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Successfully connected to MCP Server");
                }
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex,
                    "HTTP error connecting to MCP Server at {BaseUrl}. Status: {StatusCode}, Message: {Message}",
                    mcpOptions.BaseUrl,
                    ex.Data.Contains("StatusCode") ? ex.Data["StatusCode"] : "Unknown",
                    ex.Message
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to MCP Server");
                throw;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            _disposed = true;

            if (_mcpClient is not null)
            {
                await _mcpClient.DisposeAsync();
                _mcpClient = null;
            }

            _connectionLock.Dispose();
        }
    }
}