using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace SimpleMediator;

// Mediator implementation
public class Mediator(IServiceProvider serviceProvider, SimpleMediatorOptions options) : IMediator
{
    // Per-TResponse send cache to avoid repeated reflection
    private static class SendCache<TResponse>
    {
        public static readonly ConcurrentDictionary<Type, Func<IServiceProvider, object, CancellationToken, Task<TResponse>>> Cache = new();
    }

    private static readonly MethodInfo SendCoreMethod = typeof(Mediator)
        .GetMethod(nameof(SendCore), BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("Unable to locate Mediator.SendCore method.");

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        Func<IServiceProvider, object, CancellationToken, Task<TResponse>> invoker;
        try
        {
            invoker = SendCache<TResponse>.Cache.GetOrAdd(requestType, static rt => BuildSendInvoker<TResponse>(rt));
        }
        catch (Exception ex)
        {
            throw new MediatorException($"Failed to build the send pipeline for request type '{requestType}'.", ex);
        }

        return await invoker(serviceProvider, request, cancellationToken).ConfigureAwait(false);
    }

    private static Func<IServiceProvider, object, CancellationToken, Task<TResponse>> BuildSendInvoker<TResponse>(Type requestType)
    {
        // Build a compiled lambda that dispatches request into SendCore<TRequest,TResponse>.
        var spParam = System.Linq.Expressions.Expression.Parameter(typeof(IServiceProvider), "sp");
        var reqParam = System.Linq.Expressions.Expression.Parameter(typeof(object), "req");
        var ctParam = System.Linq.Expressions.Expression.Parameter(typeof(CancellationToken), "ct");

        var callCore = System.Linq.Expressions.Expression.Call(
            SendCoreMethod.MakeGenericMethod(requestType, typeof(TResponse)),
            spParam,
            System.Linq.Expressions.Expression.Convert(reqParam, requestType),
            ctParam);

        var lambda = System.Linq.Expressions.Expression.Lambda<Func<IServiceProvider, object, CancellationToken, Task<TResponse>>>(
            callCore, spParam, reqParam, ctParam);

        return lambda.Compile();
    }

    private static Task<TResponse> SendCore<TRequest, TResponse>(IServiceProvider sp, TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(sp);
        ArgumentNullException.ThrowIfNull(request);

        object handlerObj;
        try
        {
            handlerObj = RequestHandlerResolver.GetSingleHandler(sp, typeof(TRequest), typeof(TResponse));
        }
        catch (HandlerNotFoundException)
        {
            throw;
        }
        catch (MultipleHandlersFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MediatorException($"Failed to resolve handler for request type '{typeof(TRequest)}'.", ex);
        }

        var handler = (IRequestHandler<TRequest, TResponse>)handlerObj;

        IEnumerable<IPipelineBehavior<TRequest, TResponse>> behaviors;
        try
        {
            behaviors = sp.GetServices<IPipelineBehavior<TRequest, TResponse>>();
        }
        catch (Exception ex)
        {
            throw new MediatorException($"Failed to resolve pipeline behaviors for request type '{typeof(TRequest)}'.", ex);
        }

        Func<Task<TResponse>> next = () => handler.Handle(request, cancellationToken);

        // Registration order should be execution order.
        // Build chain in reverse so behaviors[0] is outermost.
        if (behaviors is not null)
        {
            var behaviorList = behaviors as IList<IPipelineBehavior<TRequest, TResponse>> ?? behaviors.ToList();
            for (var i = behaviorList.Count - 1; i >= 0; i--)
            {
                var behavior = behaviorList[i];
                var currentNext = next;
                next = () => behavior.Handle(request, currentNext, cancellationToken);
            }
        }

        return next();
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        // Resolve typed handlers and invoke directly (no reflection per handler)
        IEnumerable<INotificationHandler<TNotification>> handlers;
        try
        {
            handlers = serviceProvider.GetServices<INotificationHandler<TNotification>>();
        }
        catch (Exception ex)
        {
            throw new MediatorException($"Failed to resolve notification handlers for notification type '{typeof(TNotification)}'.", ex);
        }

        if (handlers is null) return;

        if (options.NotificationPublishMode == NotificationPublishMode.Sequential)
        {
            foreach (var handler in handlers)
            {
                await handler.Handle(notification, cancellationToken).ConfigureAwait(false);
            }

            return;
        }

        var tasks = new List<Task>(capacity: 4);
        foreach (var handler in handlers)
        {
            tasks.Add(handler.Handle(notification, cancellationToken));
        }
        if (tasks.Count == 0) return;
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
