namespace Cognitx.McpClient.Options
{
    public sealed class EntraOptions
    {
        public const string SectionName = "Entra";

        public string TenantId { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public string Instance { get; set; } = null!;
    }
}