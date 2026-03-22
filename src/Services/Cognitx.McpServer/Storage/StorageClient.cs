using Azure.Data.Tables;
using Cognitx.McpServer.Models;
using Microsoft.Extensions.Options;

namespace Cognitx.McpServer.Storage
{
    public sealed class StorageClient
    {
        private readonly StorageOptions _storageOptions;
        private readonly TableClient _tableClient;
        private readonly TimeProvider _timeProvider;

        public StorageClient(IOptions<StorageOptions> storageOptions, TimeProvider timeProvider)
        {
            _storageOptions = storageOptions.Value;
            _tableClient = CreateTableClient();
            _timeProvider = timeProvider;
        }

        private TableClient CreateTableClient()
        {
            var connectionString = _storageOptions.ConnectionString;
            var tableName = Todo.TableName;

            var client = new TableClient(connectionString, tableName);
            client.CreateIfNotExists();

            return client;
        }

        public async Task<Todo> AddTodoAsync(string userEmail, string text)
        {
            var todo = new Todo(userEmail, text, TodoStatusEnum.Pending, _timeProvider.GetUtcNow());
            await _tableClient.AddEntityAsync(todo);

            return todo;
        }

        public async Task<Todo?> GetTodoAsync(string userEmail, string todoId)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<Todo>(userEmail, todoId);
                return response.Value;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<bool> DeleteTodoAsync(string userEmail, string todoId)
        {
            try
            {
                await _tableClient.DeleteEntityAsync(userEmail, todoId);
                return true;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }

        public async Task<Todo> CompleteTodoAsync(string userEmail, string todoId)
        {
            var todo = await GetTodoAsync(userEmail, todoId);
            if (todo == null)
                throw new InvalidOperationException($"Todo with id '{todoId}' not found");

            todo.Status = TodoStatusEnum.Completed;
            await _tableClient.UpdateEntityAsync(todo, todo.ETag, TableUpdateMode.Replace);

            return todo;
        }

        public async Task<Todo> ReopenTodoAsync(string userEmail, string todoId)
        {
            var todo = await GetTodoAsync(userEmail, todoId);
            if (todo == null)
                throw new InvalidOperationException($"Todo with id '{todoId}' not found");

            todo.Status = TodoStatusEnum.Pending;
            await _tableClient.UpdateEntityAsync(todo, todo.ETag, TableUpdateMode.Replace);

            return todo;
        }

        public async Task<Todo> SetPriorityAsync(string userEmail, string todoId, int priority)
        {
            var todo = await GetTodoAsync(userEmail, todoId);
            if (todo == null)
                throw new InvalidOperationException($"Todo with id '{todoId}' not found");

            todo.Priority = priority;
            await _tableClient.UpdateEntityAsync(todo, todo.ETag, TableUpdateMode.Replace);

            return todo;
        }

        public async Task<Todo> SetDueDateAsync(string userEmail, string todoId, DateTimeOffset dueDate)
        {
            var todo = await GetTodoAsync(userEmail, todoId);
            if (todo == null)
                throw new InvalidOperationException($"Todo with id '{todoId}' not found");

            todo.DueDate = dueDate;
            await _tableClient.UpdateEntityAsync(todo, todo.ETag, TableUpdateMode.Replace);

            return todo;
        }

        public async Task<Todo> AddNoteAsync(string userEmail, string todoId, string note)
        {
            var todo = await GetTodoAsync(userEmail, todoId);
            if (todo == null)
                throw new InvalidOperationException($"Todo with id '{todoId}' not found");

            todo.Notes = string.IsNullOrWhiteSpace(todo.Notes)
                ? note
                : $"{todo.Notes}\n{note}";

            await _tableClient.UpdateEntityAsync(todo, todo.ETag, TableUpdateMode.Replace);

            return todo;
        }

        public async Task<TodoListResponse> ListTodosAsync(
            string userEmail,
            TodoStatusEnum? status = null,
            int? priority = null,
            DateTimeOffset? dueBefore = null,
            int limit = 50,
            string? cursor = null)
        {
            var results = new List<Todo>();
            var query = _tableClient.QueryAsync<Todo>(
                entity => entity.PartitionKey == userEmail);

            await foreach (var todo in query)
            {
                if (status.HasValue && status.Value != TodoStatusEnum.None && todo.Status != status.Value)
                    continue;

                if (priority.HasValue && todo.Priority != priority.Value)
                    continue;

                if (dueBefore.HasValue && (!todo.DueDate.HasValue || todo.DueDate.Value > dueBefore.Value))
                    continue;

                results.Add(todo);
            }

            var sorted = results
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate ?? DateTimeOffset.MaxValue)
                .ThenBy(t => t.CreatedUtc)
                .ToList();

            var startIndex = cursor != null && int.TryParse(cursor, out var idx) ? idx : 0;
            var paginated = sorted.Skip(startIndex).Take(limit).ToList();

            var nextCursor = startIndex + paginated.Count < sorted.Count
                ? (startIndex + paginated.Count).ToString()
                : null;

            return new(paginated, nextCursor, sorted.Count);
        }

        public async Task<TodoListResponse> FindTodosAsync(
            string userEmail,
            string query,
            TodoStatusEnum? status = null,
            int limit = 50,
            string? cursor = null)
        {
            var allResults = new List<Todo>();
            var tableQuery = _tableClient.QueryAsync<Todo>(
                entity => entity.PartitionKey == userEmail);

            await foreach (var todo in tableQuery)
            {
                if (status.HasValue && status.Value != TodoStatusEnum.None && todo.Status != status.Value)
                    continue;

                var searchText = query.ToLowerInvariant();
                if (todo.Text.ToLowerInvariant().Contains(searchText) ||
                    (todo.Notes != null && todo.Notes.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)))
                {
                    allResults.Add(todo);
                }
            }

            var sorted = allResults
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate ?? DateTimeOffset.MaxValue)
                .ThenBy(t => t.CreatedUtc)
                .ToList();

            var startIndex = cursor != null && int.TryParse(cursor, out var idx) ? idx : 0;
            var paginated = sorted.Skip(startIndex).Take(limit).ToList();

            var nextCursor = startIndex + paginated.Count < sorted.Count
                ? (startIndex + paginated.Count).ToString()
                : null;

            return new(paginated, nextCursor, sorted.Count);
        }
    }
}