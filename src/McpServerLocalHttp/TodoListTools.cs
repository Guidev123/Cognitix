using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServerLocalHttp
{
    [McpServerToolType]
    public class TodoListTools(StorageClient storageClient)
    {
        [McpServerTool, Description("Add a new todo item")]
        public async Task AddTodo([Description("The text of the todo item")] string text)
        {
            await storageClient.AddTodoAsync(text);
        }

        [McpServerTool, Description("Returns the current list of todo items")]
        public async Task<List<TodoEntity>> GetTodoList()
        {
            return await storageClient.ListTodosAsync();
        }

        [McpServerTool, Description("Delete todo item from the list")]
        public async Task DeleteTodo([Description("The id of item to delete")] string id)
        {
            await storageClient.DeleteTodoAsync(id);
        }
    }
}