using Cognitx.McpServer.Extensions;
using Cognitx.McpServer.Models;
using Cognitx.McpServer.Storage;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Cognitx.McpServer.Tools
{
    [McpServerToolType]
    public class TodoListTools(StorageClient storageClient, IHttpContextAccessor httpContextAccessor)
    {
        [McpServerTool, Description("Adds a new todo item to the list. Returns the created todo with its ID.")]
        public async Task<Todo> AddTodo([Description("The text of the todo item")] string text)
        {
            return await storageClient.AddTodoAsync(httpContextAccessor.GetUserEmail(), text);
        }

        [McpServerTool, Description("Gets a specific todo item by its ID.")]
        public async Task<Todo?> GetTodo([Description("The unique identifier of the todo item")] string id)
        {
            return await storageClient.GetTodoAsync(httpContextAccessor.GetUserEmail(), id);
        }

        [McpServerTool, Description("Marks a todo item as completed. Returns the updated todo.")]
        public async Task<Todo> CompleteTodo([Description("The unique identifier of the todo item to complete")] string id)
        {
            return await storageClient.CompleteTodoAsync(httpContextAccessor.GetUserEmail(), id);
        }

        [McpServerTool, Description("Reopens a completed todo item, changing its status back to pending. Returns the updated todo.")]
        public async Task<Todo> ReopenTodo([Description("The unique identifier of the todo item to reopen")] string id)
        {
            return await storageClient.ReopenTodoAsync(httpContextAccessor.GetUserEmail(), id);
        }

        [McpServerTool, Description("Sets the priority of a todo item. Higher numbers indicate higher priority (0 = normal, positive = higher, negative = lower). Returns the updated todo.")]
        public async Task<Todo> SetPriority(
            [Description("The unique identifier of the todo item")] string id,
            [Description("The priority value (0 = normal, positive = higher priority, negative = lower priority)")] int priority)
        {
            return await storageClient.SetPriorityAsync(httpContextAccessor.GetUserEmail(), id, priority);
        }

        [McpServerTool, Description("Sets the due date for a todo item. Returns the updated todo.")]
        public async Task<Todo> SetDueDate(
            [Description("The unique identifier of the todo item")] string id,
            [Description("The due date and time in ISO 8601 format (e.g., 2024-12-31T23:59:59Z)")] DateTimeOffset dueDate)
        {
            return await storageClient.SetDueDateAsync(httpContextAccessor.GetUserEmail(), id, dueDate);
        }

        [McpServerTool, Description("Adds a note to a todo item. Notes are appended to any existing notes. Returns the updated todo.")]
        public async Task<Todo> AddNote(
            [Description("The unique identifier of the todo item")] string id,
            [Description("The note text to add to the todo item")] string note)
        {
            return await storageClient.AddNoteAsync(httpContextAccessor.GetUserEmail(), id, note);
        }

        [McpServerTool, Description("Searches for todo items matching a query string. Supports filtering by status and pagination.")]
        public async Task<TodoListResponse> FindTodos(
            [Description("The search query to match against todo text and notes")] string query,
            [Description("Optional: Filter by status (Pending, Completed, Cancelled)")] TodoStatusEnum? status = null,
            [Description("Maximum number of results to return (default: 50)")] int limit = 50,
            [Description("Optional: Cursor for pagination (from previous FindTodos result)")] string? cursor = null)
        {
            return await storageClient.FindTodosAsync(httpContextAccessor.GetUserEmail(), query, status, limit, cursor);
        }

        [McpServerTool, Description("Gets a paginated and filtered list of todo items. Supports filtering by status, priority, and due date. Returns results with pagination cursor.")]
        public async Task<TodoListResponse> GetTodoList(
            [Description("Optional: Filter by status (Pending, Completed, Cancelled)")] TodoStatusEnum? status = null,
            [Description("Optional: Filter by exact priority value")] int? priority = null,
            [Description("Optional: Only return todos with due date before this date/time (ISO 8601 format)")] DateTimeOffset? dueBefore = null,
            [Description("Maximum number of results to return (default: 50)")] int limit = 50,
            [Description("Optional: Cursor for pagination (from previous GetTodoList result)")] string? cursor = null)
        {
            return await storageClient.ListTodosAsync(httpContextAccessor.GetUserEmail(), status, priority, dueBefore, limit, cursor);
        }

        [McpServerTool, Description("DANGEROUS: Permanently deletes a todo item from the list. This action cannot be undone. Returns true if deleted, false if not found.")]
        public async Task<bool> DeleteTodo([Description("The unique identifier of the todo item to delete")] string id)
        {
            return await storageClient.DeleteTodoAsync(httpContextAccessor.GetUserEmail(), id);
        }
    }
}