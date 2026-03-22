
# Web

React 19 + TypeScript SPA. Provides the chat interface through which users authenticate with Azure Entra ID and interact with the AI agent via the McpClient API.

## Stack

| | |
|---|---|
| Framework | React 19, TypeScript 5.8 |
| Build | Vite 6 |
| Auth | `@azure/msal-browser` v4 + `@azure/msal-react` v3 |
| Styling | Tailwind CSS v4 |
| Icons | `lucide-react` |

## Authentication

Authentication uses MSAL's public client flow:

- `PublicClientApplication` is initialized before the React tree mounts.
- `authService` holds a reference to the MSAL instance and exposes `getToken()`, which attempts a silent `acquireTokenSilent` first.
- Tokens are cached in `localStorage`.
- Login and logout use popup flows (`loginPopup` / `logoutPopup`).
- The acquired token is scoped to `api://<FRONTEND_CLIENT_ID>/access_as_user` and attached as `Authorization: Bearer` on every `/api/chat` request.

## Project Structure

```
src/
├── config/
│   └── msalConfig.ts          # MSAL Configuration and loginRequest scopes
├── components/
│   ├── Auth/
│   │   └── LoginButton.tsx    # Login/logout button with user display
│   ├── Chat/
│   │   ├── ChatContainer.tsx  # Root chat component; auth gate
│   │   ├── MessageList.tsx    # Scrollable message history
│   │   ├── MessageBubble.tsx  # Single message rendering
│   │   ├── MessageInput.tsx   # Text input with send action
│   │   └── ToolInvocation.tsx # Renders tool call metadata
│   └── Layout/
│       └── AppLayout.tsx      # Page shell: header, main, footer
├── hooks/
│   ├── useAuth.ts             # MSAL state: isAuthenticated, user, login, logout
│   └── useChat.ts             # Chat state: messages, sendMessage, clearChat
├── services/
│   ├── authService.ts         # Token acquisition wrapper around MSAL instance
│   └── chatService.ts         # POST /api/chat with bearer token
└── types/
    ├── auth.ts                # UserProfile type
    └── chat.ts                # ChatRequest, ChatResponse, Message types
```

## Environment Variables

Create a `.env.local` file at the root of `src/Web`:

```env
VITE_AZURE_TENANT_ID=<tenant-id>
VITE_FRONTEND_CLIENT_ID=<frontend-app-registration-client-id>
VITE_API_BASE_URL=<mcp-client-api-base-url>
```

All variables are prefixed with `VITE_` and inlined at build time by Vite.

## Running Locally

```bash
cd src/Web
npm install
npm run dev
```

The dev server starts on `http://localhost:5173` by default. The McpClient service must be running and its origin must be configured in `WebAppEndpoints` on the server side for CORS to pass.

## Build

```bash
npm run build
```

Output is written to `dist/`. The build runs `tsc -b` before Vite bundles, so TypeScript errors are treated as build failures.

## Entra ID App Registration Requirements

The frontend app registration must have:

- A **SPA** platform with the redirect URI set to the app's origin (e.g. `http://localhost:5173` for local dev).
- The `access_as_user` scope exposed by the McpClient app registration added as an API permission (delegated).
