using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace SimpleMediator;

// Mediator implementation
public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    // Per-TResponse send cache to avoid repeated reflection
    private static class SendCache<TResponse>
    {
        public static readonly ConcurrentDictionary<Type, Func<IServiceProvider, object, CancellationToken, Task<TResponse>>> Cache = new();
    }

    private static readonly MethodInfo GetSingleHandlerMethod = typeof(RequestHandlerResolver)
        .GetMethod(nameof(RequestHandlerResolver.GetSingleHandler), BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException("Unable to locate RequestHandlerResolver.GetSingleHandler method.");

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
        // Build a compiled lambda equivalent to resolving a single handler via RequestHandlerResolver and invoking it.
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
        return handler.Handle(request, cancellationToken);
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

        var tasks = new List<Task>(capacity: 4);
        foreach (var handler in handlers)
        {
            tasks.Add(handler.Handle(notification, cancellationToken));
        }
        if (tasks.Count == 0) return;
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
