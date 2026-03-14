namespace Cognitx.McpClient.Options
{
    public sealed class AgentOptions
    {
        public const string SectionName = "Agent";

        public string SystemPrompt { get; set; } = null!;
        public int TimeoutSeconds { get; set; }
    }
}