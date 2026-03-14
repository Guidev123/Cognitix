namespace Cognitx.McpClient.Chat
{
    public interface IAgentService
    {
        Task<ChatResponse> ProcessAsync(ChatRequest request, CancellationToken cancellationToken = default);
    }
}