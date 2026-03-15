using Cognitx.McpClient.Agent;
using Cognitx.McpClient.Interfaces;
using Cognitx.McpClient.Mcp;
using Cognitx.McpClient.Options;
using Cognitx.McpClient.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace Cognitx.McpClient.Configurations
{
    public static class ApiConfiguration
    {
        public static WebApplicationBuilder AddMcpClientInfrastructure(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(OpenAiOptions.SectionName));
            builder.Services.Configure<AgentOptions>(builder.Configuration.GetSection(AgentOptions.SectionName));
            builder.Services.Configure<McpOptions>(builder.Configuration.GetSection(McpOptions.SectionName));
            builder.Services.Configure<EntraOptions>(builder.Configuration.GetSection(EntraOptions.SectionName));
            builder.Services.AddTransient<McpAuthenticationHandler>();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddHttpClient<IMcpConnection, McpConnection>()
                .AddHttpMessageHandler<McpAuthenticationHandler>()
                .ConfigureHttpClient((sp, client) =>
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

        public static WebApplicationBuilder AddSecurity(this WebApplicationBuilder builder)
        {
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(options =>
                {
                    var entraOptions = builder.Configuration.GetSection(EntraOptions.SectionName).Get<EntraOptions>()!;
                    options.TokenValidationParameters.ValidateAudience = true;
                    options.TokenValidationParameters.ValidateIssuer = true;
                    options.TokenValidationParameters.ValidAudience = entraOptions.Audience;
                }, options =>
                {
                    var entraOptions = builder.Configuration.GetSection(EntraOptions.SectionName).Get<EntraOptions>()!;
                    options.Instance = entraOptions.Instance;
                    options.TenantId = entraOptions.TenantId;
                    options.ClientId = entraOptions.ClientId;
                    options.ClientSecret = entraOptions.ClientSecret;
                });

            builder.Services
                .AddAuthorizationBuilder()
                .AddPolicy("RequireAuthenticatedUser", policy => policy.RequireAuthenticatedUser());

            return builder;
        }
    }
}