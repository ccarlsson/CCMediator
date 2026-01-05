using System;
using System.Collections.Generic;

namespace SimpleMediator;

/// <summary>
/// Thrown when more than one handler is registered for a given request/response pair.
/// </summary>
public sealed class MultipleHandlersFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleHandlersFoundException"/> class.
    /// </summary>
    /// <param name="requestType">The request type.</param>
    /// <param name="responseType">The response type.</param>
    /// <param name="handlerTypes">The resolved handler types, if available.</param>
    public MultipleHandlersFoundException(Type requestType, Type responseType, IReadOnlyList<Type>? handlerTypes = null)
        : base($"Multiple handlers were registered for request type '{requestType.FullName}' with response type '{responseType.FullName}'.")
    {
        RequestType = requestType;
        ResponseType = responseType;
        HandlerTypes = handlerTypes ?? Array.Empty<Type>();
    }

    /// <summary>
    /// Gets the request type that had multiple handlers registered.
    /// </summary>
    public Type RequestType { get; }

    /// <summary>
    /// Gets the response type associated with the request.
    /// </summary>
    public Type ResponseType { get; }

    /// <summary>
    /// Gets the resolved handler types.
    /// </summary>
    public IReadOnlyList<Type> HandlerTypes { get; }
}
