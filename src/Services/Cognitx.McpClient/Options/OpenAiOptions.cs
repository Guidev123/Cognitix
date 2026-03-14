namespace Cognitx.McpClient.Options
{
    public sealed class OpenAiOptions
    {
        public const string SectionName = "OpenAI";

        public string ApiKey { get; set; } = null!;
        public string Model { get; set; } = null!;
    }
}