using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Shared.Middlewares;

public abstract class BaseMiddleware
{
    protected bool IsRestfulRequest(HttpContext context)
    {
        var path = context.Request.Path.Value;
        if (string.IsNullOrWhiteSpace(path) || path.Length < 7) return false;

        /* Api path: /v{x}/api... */
        var apiPathPattern = @"^/v\d+/api$";

        return Regex.IsMatch(path[..7], apiPathPattern);
    }
}