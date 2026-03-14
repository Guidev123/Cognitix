namespace Cognitx.McpServer.Storage
{
    public sealed class StorageOptions
    {
        public const string SectionName = "Storage";

        public string ConnectionString { get; set; } = null!;
    }
}