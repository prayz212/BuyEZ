using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Shared.RestAPIs.Handlers;

public class TokenAuthenticationHandlers : DelegatingHandler
{
    private readonly ILogger<TokenAuthenticationHandlers> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenAuthenticationHandlers(ILogger<TokenAuthenticationHandlers> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorization = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();
        var accessToken = authorization?.Substring("Bearer ".Length).Trim();
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            _logger.LogInformation("Adding authentication token to the request: {@Request}", request);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}