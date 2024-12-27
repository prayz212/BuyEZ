using Shared.Common.Exceptions;

using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Shared.GrpcProto.Helpers;

/* 
    Reference to Anthony's blog 
    Link: https://anthonygiretti.com/2022/08/28/asp-net-core-6-handling-grpc-exception-correctly-server-side/
*/
public static class GrpcExceptionHelpers
{
    public static RpcException ErrorHandling<T>(this Exception exception, ServerCallContext context, ILogger<T> logger, Guid correlationId) =>
        exception switch
    {
        ValidationException => HandleValidationException((ValidationException) exception, context, logger, correlationId),
        NotFoundException => HandleNotFoundException((NotFoundException) exception, context, logger, correlationId),
        UnauthorizedAccessException => HandleUnauthorizedAccessException((UnauthorizedAccessException) exception, context, logger, correlationId),
        ForbiddenException => HandleForbiddenException((ForbiddenException) exception, context, logger, correlationId),
        _ => HandleUnknownException(exception, context, logger, correlationId)
    };

    private static RpcException HandleValidationException<T>(ValidationException exception, ServerCallContext context, ILogger<T> logger, Guid correlationId)
    {
        logger.LogError(exception, $"CorrelationId: {correlationId} - An input validation error occurred on method {context.Method}");

        var status = new Status(StatusCode.InvalidArgument, exception.Message);

        return new RpcException(status, CreateTrailers(correlationId));
    }

    private static RpcException HandleNotFoundException<T>(NotFoundException exception, ServerCallContext context, ILogger<T> logger, Guid correlationId)
    {
        logger.LogError(exception, $"CorrelationId: {correlationId} - A not found error occurred");

        var status = new Status(StatusCode.NotFound, exception.Message);

        return new RpcException(status, CreateTrailers(correlationId));
    }

    private static RpcException HandleUnauthorizedAccessException<T>(UnauthorizedAccessException exception, ServerCallContext context, ILogger<T> logger, Guid correlationId)
    {
        logger.LogError(exception, $"CorrelationId: {correlationId} - An unauthorized access error occurred on method {context.Method}");

        var status = new Status(StatusCode.Unauthenticated, "Service must be authenticated to access this resource.");
        
        return new RpcException(status, CreateTrailers(correlationId));
    }

    private static RpcException HandleForbiddenException<T>(ForbiddenException exception, ServerCallContext context, ILogger<T> logger, Guid correlationId)
    {
        logger.LogError(exception, $"CorrelationId: {correlationId} - An permission denied error occurred on method {context.Method}");

        var status = new Status(StatusCode.PermissionDenied, "Service do not have permission to access this resource.");
        
        return new RpcException(status, CreateTrailers(correlationId));
    }

    private static RpcException HandleUnknownException<T>(Exception exception, ServerCallContext context, ILogger<T> logger, Guid correlationId)
    {
        logger.LogError(exception, $"CorrelationId: {correlationId} - An error occurred on method {context.Method}");
        return new RpcException(new Status(StatusCode.Internal, exception.Message), CreateTrailers(correlationId));
    }

    /// <summary>
    ///  Adding the correlation to Response Trailers
    /// </summary>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    private static Metadata CreateTrailers(Guid correlationId)
    {
        var trailers = new Metadata();
        trailers.Add("CorrelationId", correlationId.ToString());

        return trailers;
    }
}