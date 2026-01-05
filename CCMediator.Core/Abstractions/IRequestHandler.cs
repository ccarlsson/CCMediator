namespace CCMediator;

/// <summary>
/// Handles a request and produces a response.
/// </summary>
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
