using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace CatalogAPI.Application.Common.Behaviors;

public class LoggingBehavior<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;

    public LoggingBehavior(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // TODO: implement later
        // var userId = _currentUserService.UserId ?? string.Empty;

        _logger.LogInformation("Catalog API Request: {Name} {@Request}", requestName, request);
        return Task.CompletedTask;
    }
}
