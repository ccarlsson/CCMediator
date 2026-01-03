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

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        var invoker = SendCache<TResponse>.Cache.GetOrAdd(requestType, static rt => BuildSendInvoker<TResponse>(rt));
        return await invoker(serviceProvider, request, cancellationToken).ConfigureAwait(false);
    }

    private static Func<IServiceProvider, object, CancellationToken, Task<TResponse>> BuildSendInvoker<TResponse>(Type requestType)
    {
        // Build a compiled lambda equivalent to resolving a single handler via RequestHandlerResolver and invoking it.
        var spParam = System.Linq.Expressions.Expression.Parameter(typeof(IServiceProvider), "sp");
        var reqParam = System.Linq.Expressions.Expression.Parameter(typeof(object), "req");
        var ctParam = System.Linq.Expressions.Expression.Parameter(typeof(CancellationToken), "ct");

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handlerExpr = System.Linq.Expressions.Expression.Convert(
            System.Linq.Expressions.Expression.Call(
                GetSingleHandlerMethod,
                spParam,
                System.Linq.Expressions.Expression.Constant(requestType, typeof(Type)),
                System.Linq.Expressions.Expression.Constant(typeof(TResponse), typeof(Type))),
            handlerType);

        var castReq = System.Linq.Expressions.Expression.Convert(reqParam, requestType);
        var handleMethod = handlerType.GetMethod("Handle")!;

        var callHandle = System.Linq.Expressions.Expression.Call(handlerExpr, handleMethod, castReq, ctParam);
        var lambda = System.Linq.Expressions.Expression.Lambda<Func<IServiceProvider, object, CancellationToken, Task<TResponse>>>(
            callHandle, spParam, reqParam, ctParam);

        return lambda.Compile();
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        // Resolve typed handlers and invoke directly (no reflection per handler)
        var handlers = serviceProvider.GetServices<INotificationHandler<TNotification>>();
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
