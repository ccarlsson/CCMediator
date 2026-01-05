using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace SimpleMediator;

/// <summary>
/// Resolves request handlers while enforcing the "exactly one" constraint.
/// </summary>
internal static class RequestHandlerResolver
{
    public static object GetSingleHandler(IServiceProvider serviceProvider, Type requestType, Type responseType)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(requestType);
        ArgumentNullException.ThrowIfNull(responseType);

        var handlerServiceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        var handlers = serviceProvider.GetServices(handlerServiceType).ToList();

        if (handlers.Count == 0)
        {
            throw new HandlerNotFoundException(requestType, responseType);
        }

        if (handlers.Count > 1)
        {
            throw new MultipleHandlersFoundException(requestType, responseType, handlers.Select(static h => h?.GetType() ?? typeof(object)).ToArray());
        }

        return handlers[0]!;
    }
}
