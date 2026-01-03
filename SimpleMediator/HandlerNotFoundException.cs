using System;

namespace SimpleMediator;

/// <summary>
/// Thrown when no handler is registered for a given request/response pair.
/// </summary>
public sealed class HandlerNotFoundException : Exception
{
    public HandlerNotFoundException(Type requestType, Type responseType)
        : base($"No handler was registered for request type '{requestType.FullName}' with response type '{responseType.FullName}'.")
    {
        RequestType = requestType;
        ResponseType = responseType;
    }

    public Type RequestType { get; }

    public Type ResponseType { get; }
}
