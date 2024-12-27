using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shared.Middlewares;

public class ExtractTokenMiddleware(RequestDelegate next, ILogger<ExtractTokenMiddleware> logger) : BaseMiddleware
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExtractTokenMiddleware> _logger = logger;

    private const string TenantIdClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/tenantid";

    public async Task InvokeAsync(HttpContext context)
    {
        /* Only use this middleware for restful api */
        if (!IsRestfulRequest(context))
        {
            await _next(context);
            return;
        }

        _logger.LogInformation("Extracting token information...");

        var authorization = context.Request.Headers.Authorization.FirstOrDefault();
        if (authorization == null)
        {
            _logger.LogInformation("Cannot get authorization in request header. Path: {Path}", context.Request.Path);
            await _next(context);
            return;
        }

        var token = authorization.Substring("Bearer ".Length).Trim();
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(token))
        {
            _logger.LogInformation("Cannot read authorization token. Token: {Token}", token);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var jwtToken = handler.ReadJwtToken(token);
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        if (userIdClaim == null || roleClaim == null 
            || string.IsNullOrWhiteSpace(userIdClaim.Value) || string.IsNullOrWhiteSpace(roleClaim.Value))
        {
            _logger.LogInformation("Cannot find user claims in token. Token: {Token}", token);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var tenantIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == TenantIdClaimType);

        context.Items.Add("TenantId", tenantIdClaim?.Value);
        context.Items.Add("UserId", userIdClaim.Value);
        context.Items.Add("UserRole", roleClaim.Value);

        await _next(context);
    }
}