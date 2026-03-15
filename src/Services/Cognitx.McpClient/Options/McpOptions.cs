namespace Cognitx.McpClient.Options
{
    public sealed class McpOptions
    {
        public const string SectionName = "Mcp";

        public string BaseUrl { get; set; } = null!;
        public int TimeoutSeconds { get; set; }
    }
}