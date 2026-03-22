using System.Security.Claims;

namespace Cognitx.McpServer.Extensions
{
    public static class HttpContextAccessorExtensions
    {
        public static string GetUserEmail(this IHttpContextAccessor httpContextAccessor)
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)
                ?? httpContextAccessor.HttpContext?.User.FindFirst("preferred_username")
                ?? throw new InvalidOperationException("User identity claim not found.");

            return claim.Value;
        }
    }
}