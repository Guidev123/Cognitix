using Cognitx.McpClient.Options;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace Cognitx.McpClient.Chat
{
    public sealed class AgentService(
        IOptions<AgentOptions> agentOptions,
        IOptions<OpenAiOptions> openAiOptions,
        IConversationStore conversationStore,
        ILogger<AgentService> logger
        ) : IAgentService
    {
        private readonly AgentOptions _agentOptions = agentOptions.Value;
        private readonly OpenAiOptions _openAiOptions = openAiOptions.Value;
        private AIAgent? _agent;

        public async Task<ChatResponse> ProcessAsync(ChatRequest request, CancellationToken cancellationToken = default)
        {
            var conversationId = request.ConversationId ?? Guid.NewGuid().ToString("N");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_agentOptions.TimeoutSeconds));

            var timeoutToken = cts.Token;

            try
            {
                var agent = await GetOrCreateAgentAsync(timeoutToken);

                var thread = conversationStore.GetThread(conversationId);

                if (thread is null)
                {
                    thread = agent.GetNewThread();
                    conversationStore.SaveThread(conversationId, thread);
                }

                var runOptions = new AgentRunOptions();
                var result = await agent.RunAsync(request.Message, thread, options: runOptions, cancellationToken: timeoutToken);

                var responseText = result.Text ?? "I apologize, but I wasn't able to generate an answer";

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Agent completed. Response: {Response}", responseText);
                }

                return new(responseText, conversationId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing chat request");
                throw;
            }
        }

        private async Task<AIAgent> GetOrCreateAgentAsync(CancellationToken cancellationToken)
        {
            if (_agent is not null) return _agent;

            var openAiClient = new OpenAIClient(_openAiOptions.ApiKey);
            var chatClient = openAiClient.GetChatClient(_openAiOptions.Model);

            _agent = chatClient.CreateAIAgent(
                instructions: _agentOptions.SystemPrompt,
                tools: []);

            return _agent;
        }
    }
}