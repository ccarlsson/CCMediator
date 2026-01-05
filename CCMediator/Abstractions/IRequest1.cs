namespace CCMediator.Abstractions;

using CCMediator.Primitives;

/// <summary>
/// Marker interface for a request message that does not return a custom response.
/// </summary>
/// <remarks>
/// This is a convenience alias for <see cref="IRequest{TResponse}"/> with <see cref="Unit"/> as response.
/// </remarks>
public interface IRequest : IRequest<Unit> { }
