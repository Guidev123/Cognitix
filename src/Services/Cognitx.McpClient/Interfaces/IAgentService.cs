using Cognitx.McpClient.Chat;

namespace Cognitx.McpClient.Interfaces
{
    public interface IAgentService
    {
        Task<ChatResponse> ProcessAsync(ChatRequest request, CancellationToken cancellationToken = default);
    }
}