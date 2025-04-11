namespace SimpleMediator;

// Mediator implementation
public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for {requestType.Name}");

        // Use reflection to call the Handle method
        var method = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handle method not found on handler for {requestType.Name}");

        return await (Task<TResponse>)(method.Invoke(handler, [request, cancellationToken])
            ?? throw new InvalidOperationException("Handler invocation returned null"));
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        var notificationType = notification.GetType();
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);

        // Get all handlers for this notification
        var handlerWrapperType = typeof(IEnumerable<>).MakeGenericType(handlerType);
        var handlers = (IEnumerable<object>?)serviceProvider.GetService(handlerWrapperType);

        if (handlers == null) return;

        var tasks = new List<Task>();
        var method = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handle method not found on handler for {notificationType.Name}");

        foreach (var handler in handlers)
        {
            tasks.Add((Task)(method.Invoke(handler, [notification, cancellationToken])
                ?? throw new InvalidOperationException("Handler invocation returned null")));
        }
        await Task.WhenAll(tasks);
    }
}
