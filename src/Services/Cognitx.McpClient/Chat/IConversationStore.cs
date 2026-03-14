using Microsoft.Agents.AI;

namespace Cognitx.McpClient.Chat
{
    public interface IConversationStore
    {
        AgentThread? GetThread(string conversationId);

        void SaveThread(string conversationId, AgentThread thread);
    }
}