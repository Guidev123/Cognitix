# Cognitix

A reference implementation of a secured, end-to-end Model Context Protocol (MCP) system built on .NET 10 and React. The solution demonstrates a production-grade MCP architecture with Azure Entra ID as the identity plane, covering the full token delegation chain from the browser to the AI agent to the MCP server.

## Architecture Overview

```
┌─────────────────┐        ┌──────────────────────┐        ┌──────────────────────┐
│   React WebApp  │──JWT──▶│   Cognitx.McpClient  │──OBO──▶│  Cognitx.McpServer   │
│  (MSAL Browser) │        │  (Agent + MCP Client) │  JWT   │  (MCP + Azure Tables)│
└─────────────────┘        └──────────────────────┘        └──────────────────────┘
         │                            │                                 │
         │                     OpenAI API                       Azure Table Storage
         │                  (gpt-* via SDK)                       (Azurite local)
         │
   Azure Entra ID
  (token issuance)
```

The **WebApp** authenticates the user via MSAL and acquires a bearer token scoped to the McpClient API. The **McpClient** validates that token, then performs an On-Behalf-Of (OBO) exchange to acquire a downstream token scoped to `mcp.tools` before forwarding requests to the **McpServer**. The McpServer exposes MCP Tools and Resources over HTTP transport, backed by Azure Table Storage.

## Repository Structure

```
Cognitix/
├── docker/
│   └── docker-compose.yml          # Local infrastructure (Azurite)
├── src/
│   ├── Services/
│   │   ├── Cognitx.McpServer/      # MCP Server — Tools, Resources, Storage
│   │   └── Cognitx.McpClient/      # MCP Client — Agent, OBO auth, Chat API
│   └── Web/                        # React + TypeScript frontend
└── Cognitix.slnx                   # .NET solution file
```

## Components

| Component | Runtime | Description |
|---|---|---|
| `Cognitx.McpServer` | .NET 10 / ASP.NET Core | MCP server exposing Todo tools and resources over HTTP transport, protected by Entra ID JWT validation |
| `Cognitx.McpClient` | .NET 10 / ASP.NET Core | AI agent host that bridges the React frontend to the MCP server; performs OBO token exchange and drives the OpenAI chat loop |
| `Web` | React 19 / TypeScript / Vite | SPA frontend with MSAL authentication and chat UI |

## Security Model

Authentication uses a two-hop token delegation pattern:

1. **User → McpClient**: The browser acquires a token for the McpClient app registration (`access_as_user` scope) and attaches it as a `Bearer` header on every `/api/chat` request.
2. **McpClient → McpServer**: `McpAuthenticationHandler` (a `DelegatingHandler`) extracts the inbound user token and exchanges it for a downstream token via MSAL's On-Behalf-Of flow, scoped to `mcp.tools`. This token is injected into the outbound `HttpClient` before each MCP call.

Both services validate tokens using `Microsoft.Identity.Web` with `AddMicrosoftIdentityWebApi`.

## Local Development

### Prerequisites

- .NET 10 SDK
- Node.js 20+
- Docker Desktop (for Azurite)

### Infrastructure

```bash
cd docker
docker compose up -d
```

This starts **Azurite** (Azure Storage emulator) on ports `10000–10002`.

### Running the Services

Start each component independently — refer to the README inside each project for configuration details:

- [MCP Server →](src/Services/Cognitx.McpServer/README.md)
- [MCP Client →](src/Services/Cognitx.McpClient/README.md)
- [Web App →](src/Web/README.md)

## Tech Stack

| Layer | Technology |
|---|---|
| MCP SDK | `ModelContextProtocol` / `ModelContextProtocol.AspNetCore` v1.1.0 |
| AI Agent | `Microsoft.Agents.AI` + `OpenAI` SDK v2.8 |
| Identity | `Microsoft.Identity.Web` v4.5 / MSAL |
| Storage | `Azure.Data.Tables` v12.11 |
| Frontend | React 19, TypeScript 5.8, Vite 6, Tailwind CSS v4 |
| Runtime | .NET 10, Node.js 20 |
| Local infra | Azurite 3.29 |
