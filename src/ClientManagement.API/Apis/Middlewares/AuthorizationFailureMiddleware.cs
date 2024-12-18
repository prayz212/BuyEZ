using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ClientManagementAPI.Apis.Middlewares;

public class AuthorizationFailureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationFailureMiddleware> _logger;

    public AuthorizationFailureMiddleware(RequestDelegate next, ILogger<AuthorizationFailureMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
        {
            _logger.LogWarning("Unauthorized access attempt detected. Path: {Path}", context.Request.Path);
            await HandleUnauthorizedAccessException(context);
        }
        else if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
        {
            _logger.LogWarning("Forbidden access for user: {User}. Path: {Path}", 
                context.User.Identity?.Name ?? "Anonymous", context.Request.Path);

            await HandleForbiddenAccessException(context);
        }
    }

    private async Task HandleUnauthorizedAccessException(HttpContext context)
    {
        ProblemDetails details = new()
        {
            Status = StatusCodes.Status401Unauthorized,
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Title = "Unauthorized",
            Detail = "User must be authenticated to access this resource."
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync(JsonConvert.SerializeObject(details));
    }

    private async Task HandleForbiddenAccessException(HttpContext context)
    {
        ProblemDetails details = new()
        {
            Status = StatusCodes.Status403Forbidden,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Title = "Forbidden",
            Detail = "User do not have permission to access this resource."
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync(JsonConvert.SerializeObject(details));
    }
}
