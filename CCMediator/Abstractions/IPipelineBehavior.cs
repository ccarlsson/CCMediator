namespace SimpleMediator.Abstractions;

/// <summary>
/// Defines a middleware component that wraps request handling.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request and optionally invokes the next component in the pipeline.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="next">Delegate that invokes the next behavior/handler.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that completes with the response.</returns>
    Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken);
}
