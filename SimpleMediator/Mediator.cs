namespace SimpleMediator;

// Mediator implementation
public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    /// <summary>
    /// Sends a request to its corresponding <c>IRequestHandler&lt;TRequest, TResponse&gt;</c> resolved from the service provider.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
    /// <param name="request">The request instance to process.</param>
    /// <param name="cancellationToken">A token that is forwarded to the handler to observe cancellation.</param>
    /// <returns>A task that completes with the response produced by the handler.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no handler is registered for the request type, when the handler's <c>Handle</c> method
    /// cannot be found, or when the handler invocation returns <c>null</c>.
    /// </exception>
    /// <exception cref="System.Reflection.TargetInvocationException">
    /// Propagated if the underlying handler throws an exception during invocation.
    /// </exception>
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

    /// <summary>
    /// Publishes a notification to all registered <c>INotificationHandler&lt;TNotification&gt;</c> instances.
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification being published.</typeparam>
    /// <param name="notification">The notification instance to dispatch.</param>
    /// <param name="cancellationToken">A token that is forwarded to each handler to observe cancellation.</param>
    /// <returns>A task that completes when all handlers have finished processing.</returns>
    /// <remarks>
    /// If no handlers are registered for the notification type, the method returns immediately.
    /// Handlers are invoked concurrently and awaited via <c>Task.WhenAll</c>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a handler's <c>Handle</c> method cannot be found, or when a handler invocation returns <c>null</c>.
    /// </exception>
    /// <exception cref="System.AggregateException">
    /// When multiple handlers throw, awaiting this method may surface an <see cref="System.AggregateException"/> containing all failures.
    /// For a single failing handler, its original exception is propagated.
    /// </exception>
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
