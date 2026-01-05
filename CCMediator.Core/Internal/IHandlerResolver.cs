namespace CCMediator.Internal;

/// <summary>
/// Abstraction for resolving request and notification handlers.
/// Core is DI-container agnostic; DI integration packages provide an implementation.
/// </summary>
public interface IHandlerResolver
{
    object GetSingleRequestHandler(Type requestType, Type responseType);

    IEnumerable<object> GetPipelineBehaviors(Type requestType, Type responseType);

    IEnumerable<object> GetNotificationHandlers(Type notificationType);
}
