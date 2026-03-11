using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServerLocalHttp
{
    [McpServerToolType]
    public class DemoTools
    {
        [McpServerTool, Description("Echoes the input message back to the caller")]
        public static string Echo([Description("The message to echo")] string message)
        {
            return message;
        }

        [McpServerTool, Description("Returns the current UTC date and time in ISO 8601 format")]
        public static string UtcNow()
        {
            return DateTime.UtcNow.ToString("o");
        }
    }
}