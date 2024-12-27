using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Shared.GrpcProto.Helpers;

namespace Shared.GrpcProto;

public class GrpcExceptionInterceptor : Interceptor
{
    private readonly ILogger<GrpcExceptionInterceptor> _logger;
    private readonly Guid _correlationId;

    public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger)
    {
        _logger = logger;
        _correlationId = Guid.NewGuid();
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation($"Method: {context.Method} received request.");

        try
        {
            var response = await continuation(request, context);
            
            _logger.LogInformation($"Method: {context.Method} received response with status {context.Status}.");

            return response;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Method {context.Method} had unhandled exception: {exception.Message}");

            throw exception.ErrorHandling(context, _logger, _correlationId);
        }
    }
}