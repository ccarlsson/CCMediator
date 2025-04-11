namespace SimpleMediator;

// Interface for defining a request without a response (command)
public interface IRequest : IRequest<Unit> { }
