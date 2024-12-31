using Shared.Options;
using Shared.Common.Exceptions;

using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Shared.GrpcProto;

public class GrpcApiKeyInterceptor(ILogger<GrpcApiKeyInterceptor> logger, IOptions<GrpcServerOptions> grpcOptions) : Interceptor
{
    private readonly ILogger<GrpcApiKeyInterceptor> _logger = logger;
    private readonly GrpcServerOptions _grpcOptions = grpcOptions.Value;

    private readonly string ServicePrefix = "Shared.GrpcProto.";

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation($"Validating API Key...");

        // Authenticate
        var apiKey = context.RequestHeaders.FirstOrDefault(md => md.Key == "x-api-key");
        if (apiKey == null || !_grpcOptions.ApiKeys.ContainsKey(apiKey.Value))
        {
            _logger.LogInformation($"{nameof(GrpcApiKeyInterceptor)}: Unauthenticated API Key.");
            throw new UnauthorizedAccessException();
        }
        _logger.LogInformation($"{nameof(GrpcApiKeyInterceptor)}: Authenticated API Key.");

        // Authorize
        var allowedServices = _grpcOptions.ApiKeys[apiKey.Value];
        var requestingService = context.Method.Split("/")[1].Substring(ServicePrefix.Length);
        if (allowedServices == null || !allowedServices.Any(s => s.Equals(requestingService, StringComparison.InvariantCultureIgnoreCase)))
        {
            _logger.LogInformation($"{nameof(GrpcApiKeyInterceptor)}: Unauthorized API Key ({apiKey.Value}) with service ({requestingService}).");
            throw new ForbiddenException();
        }
        _logger.LogInformation($"{nameof(GrpcApiKeyInterceptor)}: Authorized API Key.");

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