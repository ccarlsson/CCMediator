using System;
using System.Collections.Generic;

namespace SimpleMediator;

/// <summary>
/// Thrown when more than one handler is registered for a given request/response pair.
/// </summary>
public sealed class MultipleHandlersFoundException : Exception
{
    public MultipleHandlersFoundException(Type requestType, Type responseType, IReadOnlyList<Type>? handlerTypes = null)
        : base($"Multiple handlers were registered for request type '{requestType.FullName}' with response type '{responseType.FullName}'.")
    {
        RequestType = requestType;
        ResponseType = responseType;
        HandlerTypes = handlerTypes ?? Array.Empty<Type>();
    }

    public Type RequestType { get; }

    public Type ResponseType { get; }

    public IReadOnlyList<Type> HandlerTypes { get; }
}
