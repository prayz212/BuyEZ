using MediatR;

using Microsoft.Extensions.Logging;

namespace Shared.Common.Publishers;

public class FaultTolerantNotificationPublisher(
    ILogger<FaultTolerantNotificationPublisher> logger) 
    : INotificationPublisher
{
    private readonly ILogger<FaultTolerantNotificationPublisher> _logger = logger;

    public async Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
    {
        foreach (var executor in handlerExecutors)
        {
            try
            {
                // HandlerCallback invokes the actual INotificationHandler<T>
                await executor.HandlerCallback(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                var notificationName = typeof(INotification).Name;

                _logger.LogError(ex, "Unhandled Exception for Notification {NotificationName} {@Notification}", notificationName, notification);

                throw;
            }
        }
    }
}