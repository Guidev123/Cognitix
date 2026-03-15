using Cognitx.McpClient.Options;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;

namespace Cognitx.McpClient.Mcp
{
    public sealed class McpAuthenticationHandler(
        IOptions<EntraOptions> entraOptions,
        IOptions<McpOptions> mcpOptions,
        IHttpContextAccessor httpContextAccessor,
        ILogger<McpAuthenticationHandler> logger
        ) : DelegatingHandler
    {
        private readonly EntraOptions _entraOptions = entraOptions.Value;
        private readonly McpOptions _mcpOptions = mcpOptions.Value;
        private readonly SemaphoreSlim _appLock = new(1, 1);
        private IConfidentialClientApplication? _app;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var userToken = GetUserToken()
                    ?? throw new InvalidOperationException("No valid user token found in HttpContextAccessor");

                var app = await GetOrCreateAppAsync(cancellationToken);

                var userAssertion = new UserAssertion(userToken);
                var result = await app
                    .AcquireTokenOnBehalfOf([_mcpOptions.Scope], userAssertion)
                    .ExecuteAsync(cancellationToken);

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            }
            catch (MsalException msalEx)
            {
                logger.LogError(msalEx, "MSAL error during OBO token exchange: {Message}", msalEx.Message);
                throw new InvalidOperationException($"Token exchange failed: {msalEx.ErrorCode}", msalEx);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Something has failed during OBO token exchange");
                throw;
            }

            var response = await base.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "MCP Server returned {StatusCode} for {Method} {Uri}",
                    (int)response.StatusCode,
                    request.Method,
                    request.RequestUri);
            }

            return response;
        }

        private async Task<IConfidentialClientApplication> GetOrCreateAppAsync(CancellationToken cancellationToken)
        {
            if (_app is not null) return _app;

            await _appLock.WaitAsync(cancellationToken);

            try
            {
                _app = ConfidentialClientApplicationBuilder
                    .Create(_entraOptions.ClientId)
                    .WithClientSecret(_entraOptions.ClientSecret)
                    .WithAuthority(AzureCloudInstance.AzurePublic, _entraOptions.TenantId)
                    .Build();

                return _app;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail to create Confidential Client Application");
                throw;
            }
            finally
            {
                _appLock.Release();
            }
        }

        private string? GetUserToken()
        {
            var bearerHeader = "Bearer ";

            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext is null) return null;
            if (httpContext.User.Identity?.IsAuthenticated is true) return null;

            var authHeader = httpContext.Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authHeader)
                || !authHeader.StartsWith(bearerHeader, StringComparison.InvariantCultureIgnoreCase)) return null;

            return authHeader[bearerHeader.Length..].Trim();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _appLock.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}