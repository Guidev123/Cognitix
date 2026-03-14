namespace Cognitx.McpServer.Models
{
    public sealed record TodoListResponse(
        List<Todo> Items,
        string? NextCursor,
        int TotalCount
        );
}