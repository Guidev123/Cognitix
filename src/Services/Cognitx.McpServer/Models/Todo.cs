using Azure;
using Azure.Data.Tables;
using System.Text.Json.Serialization;

namespace Cognitx.McpServer.Models
{
    public sealed class Todo : ITableEntity
    {
        public const string TableName = "Todos";

        [JsonConstructor]
        public Todo()
        { }

        public Todo(string userEmail, string text, TodoStatusEnum status, DateTimeOffset createdUtc)
        {
            PartitionKey = userEmail;
            RowKey = Guid.NewGuid().ToString();
            Text = text;
            Status = status;
            CreatedUtc = createdUtc;
        }

        public string Id => RowKey;
        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public string Text { get; set; } = null!;
        public DateTimeOffset CreatedUtc { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public TodoStatusEnum Status { get; set; } = TodoStatusEnum.Pending;
        public int Priority { get; set; } = 0;
        public DateTimeOffset? DueDate { get; set; }
        public string? Notes { get; set; }

        public void UpdatePriority(int priority)
        {
            Priority = priority;
        }
    }
}