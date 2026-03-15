using Microsoft.Agents.AI;

namespace Cognitx.McpClient.Interfaces
{
    public interface IConversationStore
    {
        AgentThread? GetThread(string conversationId);

        void SaveThread(string conversationId, AgentThread thread);
    }
}