using Shared.Options;

using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Shared.GrpcProto;

public class GrpcApiKeyInterceptor(ILogger<GrpcApiKeyInterceptor> logger, IOptions<GrpcOptions> grpcOptions) : Interceptor
{
    private readonly ILogger<GrpcApiKeyInterceptor> _logger = logger;
    private readonly GrpcOptions _grpcOptions = grpcOptions.Value;

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation($"Validating API Key...");

        var apiKey = context.RequestHeaders.FirstOrDefault(md => md.Key == "x-api-key");
        if (apiKey == null || !apiKey.Value.Equals(_grpcOptions.ApiKey))
        {
            _logger.LogInformation($"API Key Rejected.");
            throw new UnauthorizedAccessException();
        }
        _logger.LogInformation($"API Key Accepted.");

        /* Temporary disable this IP restriction */
        // _logger.LogInformation($"Validating IP Address...");
        // var clientIpAddress = ExtractIpAddress(context.Peer);
        // if (!_grpcOptions.AllowedIPs.Contains(clientIpAddress))
        // {
        //     _logger.LogInformation($"IP Address is not allowed.");
        //     throw new ForbiddenException();
        // }
        // _logger.LogInformation($"IP Address is allowed.");

        return await continuation(request, context);
    }

    private string ExtractIpAddress(string peer)
    {
        // Extract IP from the Peer string (format: ipv4:192.168.1.10:port)
        if (peer.StartsWith("ipv4:") || peer.StartsWith("ipv6:"))
        {
            var parts = peer.Split(':');
            return peer.Substring(parts[0].Length + 1);
        }

        return peer;        
    }
}