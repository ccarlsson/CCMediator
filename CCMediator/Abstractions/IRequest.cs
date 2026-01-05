namespace SimpleMediator.Abstractions;

/// <summary>
/// Marker interface for a request message that returns a response.
/// </summary>
/// <typeparam name="TResponse">The response type produced by the request handler.</typeparam>
public interface IRequest<TResponse> { }
