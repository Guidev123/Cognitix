# Cognitx.McpClient

ASP.NET Core service acting as an AI agent host and MCP client. It exposes a single `/api/chat` HTTP endpoint to the frontend, manages conversation state in memory, and drives a tool-calling loop against the MCP server using `Microsoft.Agents.AI` on top of the OpenAI SDK.

## Overview

The McpClient sits between the React frontend and the MCP server. On each request it:

1. Validates the inbound Entra ID JWT (`access_as_user` scope).
2. Lazily initializes the `AIAgent` and discovers available MCP tools from the server.
3. Resolves or creates an `AgentThread` for the conversation ID.
4. Runs the agent loop — the LLM decides which MCP tools to call, the client executes them, and the final text response is returned.

All outbound calls to the MCP server are authenticated via a custom `DelegatingHandler` that performs an On-Behalf-Of token exchange.

## Components

### `AgentService`

Singleton-scoped AI agent host. Responsibilities:

- Builds and caches a single `AIAgent` instance (lazy, thread-safe via double-checked locking pattern).
- Calls `ListToolsAsync` on the `IMcpConnection` and registers each discovered MCP tool as an `AITool` via `AIFunctionFactory.Create`, embedding the tool's JSON Schema into the description so the LLM can reason about parameters.
- Maintains a `FrozenDictionary<string, McpClientTool>` registry for O(1) tool lookup during invocation.
- Delegates tool execution to `InvokeMcpToolAsync`, which calls `McpConnection.Client.CallToolAsync`.
- Applies a configurable timeout (`AgentOptions.TimeoutSeconds`) via a linked `CancellationTokenSource`.

### `McpConnection`

Manages the lifecycle of the `ModelContextProtocol.Client.McpClient` over `HttpClientTransport`. Connection establishment is protected by a `SemaphoreSlim` to avoid concurrent initialization races. The underlying `HttpClient` is pre-configured with the MCP server base URL and timeout, and has `McpAuthenticationHandler` injected as a message handler.

### `McpAuthenticationHandler`

`DelegatingHandler` that performs the On-Behalf-Of token exchange on every outbound HTTP call to the MCP server:

1. Extracts the user's bearer token from `IHttpContextAccessor`.
2. Lazily builds and caches an `IConfidentialClientApplication` using `ConfidentialClientApplicationBuilder`.
3. Calls `AcquireTokenOnBehalfOf` with the configured `mcp.tools` scope.
4. Injects the resulting access token as `Authorization: Bearer` on the outbound request.

### `ConversationStore`

In-memory `ConcurrentDictionary<string, AgentThread>` keyed by conversation ID. The conversation ID is either client-supplied or generated as a new `Guid` on the first turn.

### `ChatEndpoint`

Minimal API endpoint group under `/api`:

| Method | Path | Auth |
|---|---|---|
| `POST` | `/api/chat` | `RequireAuthenticatedUser` |

Request:
```json
{ "message": "string", "conversationId": "string?" }
```

Response:
```json
{ "answer": "string", "conversationId": "string" }
```

Returns `408` on timeout, `500` on unhandled agent errors.

## Authentication & Authorization

Inbound requests are validated using `Microsoft.Identity.Web` (`AddMicrosoftIdentityWebApi`). The `audience`, `issuer`, and `tenantId` are configured via `EntraOptions`. The authorization policy `RequireAuthenticatedUser` is applied to the chat endpoint group.

CORS is configured to allow the origin specified in `WebAppEndpoints` — set this to the React app URL.

## Configuration

`appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "<openai-api-key>",
    "Model": "<model-id>"
  },
  "Agent": {
    "SystemPrompt": "<system prompt text>",
    "TimeoutSeconds": 120
  },
  "WebAppEndpoints": "<react-app-origin>",
  "Mcp": {
    "BaseUrl": "<mcp-server-base-url>",
    "Scope": "api://<mcp-server-client-id>/mcp.tools",
    "TimeoutSeconds": 120
  },
  "Entra": {
    "TenantId": "<tenant-id>",
    "ClientId": "<mcp-client-app-registration-client-id>",
    "ClientSecret": "<client-secret>",
    "Audience": "api://<mcp-client-app-registration-client-id>",
    "Instance": "https://login.microsoftonline.com/"
  }
}
```

## Running Locally

```bash
cd src/Services/Cognitx.McpClient
dotnet run
```

The MCP server must be running and reachable at the URL configured in `Mcp.BaseUrl` before the agent initializes.

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| `ModelContextProtocol` | 1.1.0 | MCP client and `HttpClientTransport` |
| `Microsoft.Agents.AI` | 1.0.0-preview | `AIAgent`, `AgentThread`, `AgentRunOptions` |
| `Microsoft.Agents.AI.OpenAI` | 1.0.0-preview | `CreateAIAgent` extension for `OpenAIChatClient` |
| `OpenAI` | 2.8.0 | `OpenAIClient`, `ChatClient` |
| `Microsoft.Identity.Client` | 4.83.1 | MSAL — OBO flow via `IConfidentialClientApplication` |
| `Microsoft.Identity.Web` | 4.5.0 | Inbound JWT validation |
| `Microsoft.AspNetCore.OpenApi` | 10.0.4 | OpenAPI document generation |
