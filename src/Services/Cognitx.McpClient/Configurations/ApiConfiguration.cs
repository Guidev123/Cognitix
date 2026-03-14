using Cognitx.McpClient.Chat;
using Cognitx.McpClient.Options;

namespace Cognitx.McpClient.Configurations
{
    public static class ApiConfiguration
    {
        public static WebApplicationBuilder AddMcpClientInfrastructure(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(OpenAiOptions.SectionName));
            builder.Services.Configure<AgentOptions>(builder.Configuration.GetSection(AgentOptions.SectionName));

            builder.Services.AddSingleton<IConversationStore, ConversationStore>();
            builder.Services.AddScoped<IAgentService, AgentService>();

            return builder;
        }
    }
}