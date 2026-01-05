using CCMediator.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace CCMediator;

internal sealed class ServiceProviderHandlerResolver(IServiceProvider serviceProvider) : IHandlerResolver
{
    public object GetSingleRequestHandler(Type requestType, Type responseType)
    {
        var handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        var handlers = serviceProvider.GetServices(handlerServiceType).ToList();

        if (handlers.Count == 0)
        {
            throw new HandlerNotFoundException(requestType, responseType);
        }

        if (handlers.Count > 1)
        {
            throw new MultipleHandlersFoundException(
                requestType,
                responseType,
                handlers.Select(static h => h?.GetType() ?? typeof(object)).ToArray());
        }

        return handlers[0]!;
    }

    public IEnumerable<object> GetPipelineBehaviors(Type requestType, Type responseType)
    {
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        return serviceProvider.GetServices(behaviorType).Cast<object>();
    }

    public IEnumerable<object> GetNotificationHandlers(Type notificationType)
    {
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        return serviceProvider.GetServices(handlerType).Cast<object>();
    }
}
