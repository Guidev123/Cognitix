# Cognitx.McpServer

ASP.NET Core MCP server exposing a Todo management domain over HTTP transport. Authentication is enforced via Azure Entra ID JWT validation; only requests carrying a valid token with the `mcp.tools` scope are admitted.

## Overview

The server is built on `ModelContextProtocol.AspNetCore` and registers two primitive types:

- **Tools** (`TodoListTools`): stateful operations against Azure Table Storage.
- **Resources** (`TodoResources`): read-only MCP resources exposing system metadata and live statistics.

## MCP Primitives

### Tools — `TodoListTools`

| Tool | Description |
|---|---|
| `AddTodo` | Creates a new todo item. Returns the created entity with its generated ID. |
| `GetTodo` | Retrieves a single todo by ID. |
| `CompleteTodo` | Transitions a todo to `Completed` status. |
| `ReopenTodo` | Reverts a todo from `Completed` back to `Pending`. |
| `SetPriority` | Sets an integer priority on a todo (positive = higher, negative = lower, 0 = normal). |
| `SetDueDate` | Assigns an ISO 8601 due date to a todo. |
| `AddNote` | Appends a note string to a todo's notes field. |
| `GetTodoList` | Paginated list with optional filters: status, priority, `dueBefore`. |
| `FindTodos` | Full-text search across `Text` and `Notes` fields, with optional status filter and pagination. |
| `DeleteTodo` | Permanently deletes a todo. Irreversible. |

### Resources — `TodoResources`

| URI | MIME Type | Description |
|---|---|---|
| `todo://schema` | `application/schema+json` | JSON Schema for the `Todo` entity, generated at runtime via NJsonSchema. |
| `todo://stats/dashboard` | `text/markdown` | Live dashboard: total count, status breakdown, high-priority count, overdue count. |

## Data Model

```csharp
Todo {
  Id          : string          // RowKey (GUID)
  Text        : string          // Task description
  Status      : TodoStatusEnum  // None | Pending | Completed | Cancelled
  Priority    : int             // 0 = normal; higher = more urgent
  DueDate     : DateTimeOffset? // Optional
  Notes       : string?         // Appended via AddNote
  CreatedUtc  : DateTimeOffset
}
```

All entities share a single partition key (`default`). Ordering in list/search results is: priority descending → due date ascending → creation date ascending.

## Storage

Azure Table Storage via `Azure.Data.Tables`. The `StorageClient` is registered as a singleton and creates the `Todos` table on startup if it does not exist. For local development, point the connection string at Azurite.

Pagination is cursor-based using a simple integer offset serialized as a string.

## Authentication & Authorization

Token validation uses `Microsoft.Identity.Web` (`AddMicrosoftIdentityWebApi`). The MCP endpoint is protected by the `McpToolsScope` policy, which requires:

- An authenticated user (`RequireAuthenticatedUser`)
- The `mcp.tools` scope (`RequireScope("mcp.tools")`)

The `PORT` environment variable controls the listening port (default: `8080`).

## Configuration

`appsettings.json`:

```json
{
  "Storage": {
    "ConnectionString": "<Azure Table Storage connection string or Azurite>"
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<tenant-id>",
    "ClientId": "<server-app-registration-client-id>",
    "Audience": "api://<server-app-registration-client-id>",
    "Scopes": "mcp.tools"
  }
}
```

For local development with Azurite:

```
ConnectionString: UseDevelopmentStorage=true
```

## Running Locally

```bash
cd src/Services/Cognitx.McpServer
dotnet run
```

The MCP endpoint is available at `http://localhost:<PORT>/mcp`.

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| `ModelContextProtocol.AspNetCore` | 1.1.0 | MCP HTTP transport and primitive registration |
| `Azure.Data.Tables` | 12.11.0 | Table Storage client |
| `Microsoft.Identity.Web` | 4.5.0 | Entra ID JWT validation |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.5 | JWT middleware |
| `NJsonSchema` | 11.5.2 | Runtime JSON Schema generation for the schema resource |
