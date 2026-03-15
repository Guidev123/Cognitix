using System.Text.Json.Serialization;

namespace Cognitx.McpServer.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TodoStatusEnum
    {
        None,
        Pending,
        Completed,
        Cancelled
    }
}