namespace CCMediator;

/// <summary>
/// Represents errors that occur within the mediator infrastructure.
/// </summary>
public sealed class MediatorException : Exception
{
    public MediatorException(string message, Exception innerException) : base(message, innerException) { }
    public MediatorException(string message) : base(message) { }
    public MediatorException() { }
}
