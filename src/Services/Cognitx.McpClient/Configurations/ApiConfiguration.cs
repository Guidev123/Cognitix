using Cognitx.McpClient.Agent;
using Cognitx.McpClient.Interfaces;
using Cognitx.McpClient.Mcp;
using Cognitx.McpClient.Options;
using Cognitx.McpClient.Storage;

namespace Cognitx.McpClient.Configurations
{
    public static class ApiConfiguration
    {
        public static WebApplicationBuilder AddMcpClientInfrastructure(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(OpenAiOptions.SectionName));
            builder.Services.Configure<AgentOptions>(builder.Configuration.GetSection(AgentOptions.SectionName));
            builder.Services.Configure<McpOptions>(builder.Configuration.GetSection(McpOptions.SectionName));

            builder.Services.AddHttpClient<IMcpConnection, McpConnection>().ConfigureHttpClient((sp, client) =>
            {
                var options = builder.Configuration.GetSection(McpOptions.SectionName).Get<McpOptions>()
                    ?? throw new InvalidOperationException("MCP Options can not be null");

                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

            builder.Services.AddSingleton<IConversationStore, ConversationStore>();
            builder.Services.AddScoped<IAgentService, AgentService>();

            return builder;
        }
    }
}