using System;

namespace SimpleMediator.Exceptions;

/// <summary>
/// Thrown when no handler is registered for a given request/response pair.
/// </summary>
public sealed class HandlerNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class.
    /// </summary>
    /// <param name="requestType">The request type that was sent.</param>
    /// <param name="responseType">The expected response type.</param>
    public HandlerNotFoundException(Type requestType, Type responseType)
        : base($"No handler was registered for request type '{requestType.FullName}' with response type '{responseType.FullName}'.")
    {
        RequestType = requestType;
        ResponseType = responseType;
    }

    /// <summary>
    /// Gets the request type that had no handler registered.
    /// </summary>
    public Type RequestType { get; }

    /// <summary>
    /// Gets the response type associated with the request.
    /// </summary>
    public Type ResponseType { get; }
}
