namespace CCMediator.Internal;

/// <summary>
/// Abstraction for resolving request handlers, notification handlers, and pipeline behaviors.
/// </summary>
/// <remarks>
/// The core package is DI-container agnostic; integration packages (e.g. the Microsoft DI package)
/// provide concrete implementations.
/// </remarks>
public interface IHandlerResolver
{
    /// <summary>
    /// Resolves the single registered request handler for a given request/response pair.
    /// </summary>
    /// <param name="requestType">The concrete request type.</param>
    /// <param name="responseType">The concrete response type.</param>
    /// <returns>The resolved handler instance.</returns>
    /// <exception cref="HandlerNotFoundException">Thrown when no handler is registered.</exception>
    /// <exception cref="MultipleHandlersFoundException">Thrown when more than one handler is registered.</exception>
    object GetSingleRequestHandler(Type requestType, Type responseType);

    /// <summary>
    /// Resolves pipeline behaviors for the given request/response pair.
    /// </summary>
    /// <param name="requestType">The concrete request type.</param>
    /// <param name="responseType">The concrete response type.</param>
    /// <returns>Zero or more resolved behavior instances.</returns>
    IEnumerable<object> GetPipelineBehaviors(Type requestType, Type responseType);

    /// <summary>
    /// Resolves all notification handlers for a given notification type.
    /// </summary>
    /// <param name="notificationType">The concrete notification type.</param>
    /// <returns>Zero or more resolved handler instances.</returns>
    IEnumerable<object> GetNotificationHandlers(Type notificationType);
}
