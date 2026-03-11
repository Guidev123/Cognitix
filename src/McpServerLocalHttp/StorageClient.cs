using Azure.Data.Tables;

namespace McpServerLocalHttp
{
    public class StorageClient
    {
        private readonly TableClient _tableClient;

        public StorageClient()
        {
            _tableClient = CreateTableClient();
        }

        private TableClient CreateTableClient()
        {
            var connectionString = StorageConfig.ConnectionString;
            var tableName = "Todos";

            var client = new TableClient(
                "UseDevelopmentStorage=true",
                "Todos"
            );
            client.CreateIfNotExists();

            return client;
        }

        public async Task AddTodoAsync(string text)
        {
            var todo = new TodoEntity
            {
                PartitionKey = "default",
                RowKey = Guid.NewGuid().ToString(),
                Text = text,
                CreatedOn = DateTimeOffset.UtcNow
            };

            await _tableClient.AddEntityAsync(todo);
        }

        public async Task<List<TodoEntity>> ListTodosAsync()
        {
            var results = new List<TodoEntity>();

            await foreach (var todo in _tableClient.QueryAsync<TodoEntity>(
                t => t.PartitionKey == "default"))
            {
                results.Add(todo);
            }

            return results
                .OrderBy(t => t.CreatedOn)
                .ToList();
        }

        public async Task DeleteTodoAsync(string todoId)
        {
            await _tableClient.DeleteEntityAsync("default", todoId);
        }
    }
}