namespace SimpleMediator;

/// <summary>
/// Represents errors that occur within the mediator infrastructure (e.g., handler resolution,
/// dispatch/pipeline construction). User handler exceptions are typically not wrapped.
/// </summary>
public sealed class MediatorException : Exception
{
    public MediatorException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public MediatorException(string message) : base(message)
    {
    }

    public MediatorException()
    {
    }
}
