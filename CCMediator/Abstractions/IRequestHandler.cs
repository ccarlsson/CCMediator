namespace CCMediator;

/// <summary>
/// Handles a request and produces a response.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the given request.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that completes with the response.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
