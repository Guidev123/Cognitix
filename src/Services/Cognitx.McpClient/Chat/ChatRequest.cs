namespace Cognitx.McpClient.Chat
{
    public sealed record ChatRequest(string Message, string? ConversationId);
}