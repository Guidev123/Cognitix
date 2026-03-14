using Microsoft.Agents.AI;
using System.Collections.Concurrent;

namespace Cognitx.McpClient.Chat
{
    public sealed class ConversationStore : IConversationStore
    {
        private readonly ConcurrentDictionary<string, AgentThread> _threads = new();

        public AgentThread? GetThread(string conversationId)
        {
            _threads.TryGetValue(conversationId, out var thread);
            return thread;
        }

        public void SaveThread(string conversationId, AgentThread thread)
        {
            _threads[conversationId] = thread;
        }
    }
}