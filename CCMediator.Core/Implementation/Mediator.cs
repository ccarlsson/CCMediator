using CCMediator.Internal;
using System.Collections.Concurrent;
using System.Reflection;

namespace CCMediator.Implementation;

/// <summary>
/// Default <see cref="IMediator"/> implementation.
/// </summary>
public class Mediator(IHandlerResolver resolver, CCMediatorOptions options) : IMediator
{
    /// <summary>
    /// Initializes a new <see cref="Mediator"/> with default options.
    /// </summary>
    public Mediator(IHandlerResolver resolver) : this(resolver, new CCMediatorOptions())
    {
    }

    // Per-TResponse send cache to avoid repeated reflection
    private static class SendCache<TResponse>
    {
        public static readonly ConcurrentDictionary<Type, Func<IHandlerResolver, object, CancellationToken, Task<TResponse>>> Cache = new();
    }

    private static readonly MethodInfo SendCoreMethod = typeof(Mediator)
        .GetMethod(nameof(SendCore), BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("Unable to locate Mediator.SendCore method.");

    /// <inheritdoc />
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        Func<IHandlerResolver, object, CancellationToken, Task<TResponse>> invoker;
        try
        {
            invoker = SendCache<TResponse>.Cache.GetOrAdd(requestType, static rt => BuildSendInvoker<TResponse>(rt));
        }
        catch (Exception ex)
        {
            throw new MediatorException($"Failed to build the send pipeline for request type '{requestType}'.", ex);
        }

        return await invoker(resolver, request, cancellationToken).ConfigureAwait(false);
    }

    private static Func<IHandlerResolver, object, CancellationToken, Task<TResponse>> BuildSendInvoker<TResponse>(Type requestType)
    {
        var rParam = System.Linq.Expressions.Expression.Parameter(typeof(IHandlerResolver), "r");
        var reqParam = System.Linq.Expressions.Expression.Parameter(typeof(object), "req");
        var ctParam = System.Linq.Expressions.Expression.Parameter(typeof(CancellationToken), "ct");

        var callCore = System.Linq.Expressions.Expression.Call(
            SendCoreMethod.MakeGenericMethod(requestType, typeof(TResponse)),
            rParam,
            System.Linq.Expressions.Expression.Convert(reqParam, requestType),
            ctParam);

        var lambda = System.Linq.Expressions.Expression.Lambda<Func<IHandlerResolver, object, CancellationToken, Task<TResponse>>>(
            callCore, rParam, reqParam, ctParam);

        return lambda.Compile();
    }

    private static Task<TResponse> SendCore<TRequest, TResponse>(IHandlerResolver resolver, TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(request);

        object handlerObj;
        try
        {
            handlerObj = resolver.GetSingleRequestHandler(typeof(TRequest), typeof(TResponse));
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

        IEnumerable<object>? behaviors;
        try
        {
            behaviors = resolver.GetPipelineBehaviors(typeof(TRequest), typeof(TResponse));
        }
        catch
        {
            behaviors = null;
        }

        Func<Task<TResponse>> next = () => handler.Handle(request, cancellationToken);

        if (behaviors is not null)
        {
            var list = behaviors as IList<object> ?? behaviors.ToList();
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var behavior = (IPipelineBehavior<TRequest, TResponse>)list[i];
                var currentNext = next;
                next = () => behavior.Handle(request, currentNext, cancellationToken);
            }
        }

        return next();
    }

    /// <inheritdoc />
    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        IEnumerable<object> handlers;
        try
        {
            handlers = resolver.GetNotificationHandlers(typeof(TNotification));
        }
        catch (Exception ex)
        {
            throw new MediatorException($"Failed to resolve notification handlers for notification type '{typeof(TNotification)}'.", ex);
        }

        var typedHandlers = handlers.Cast<INotificationHandler<TNotification>>();

        if (options.NotificationPublishMode == NotificationPublishMode.Sequential)
        {
            if (options.SequentialPublishErrorHandling == NotificationPublishErrorHandling.StopOnFirstException)
            {
                foreach (var handler in typedHandlers)
                {
                    await handler.Handle(notification, cancellationToken).ConfigureAwait(false);
                }

                return;
            }

            var exceptions = new List<Exception>();
            foreach (var handler in typedHandlers)
            {
                try
                {
                    await handler.Handle(notification, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count == 1)
            {
                throw exceptions[0];
            }

            if (exceptions.Count > 1)
            {
                throw new AggregateException(exceptions);
            }

            return;
        }

        var tasks = typedHandlers.Select(h => h.Handle(notification, cancellationToken)).ToList();
        if (tasks.Count == 0) return;

        if (options.AggregateExceptionsInParallel)
        {
            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not AggregateException)
            {
                throw new AggregateException(ex);
            }
            return;
        }

        // Stop-on-first in parallel: return as soon as one handler faults.
        // Remaining handlers will still run, but the caller gets the first observed exception.
        var remaining = tasks;
        while (remaining.Count > 0)
        {
            var completed = await Task.WhenAny(remaining).ConfigureAwait(false);
            remaining.Remove(completed);

            // Throw immediately if this one faulted/canceled.
            await completed.ConfigureAwait(false);
        }
    }
}
