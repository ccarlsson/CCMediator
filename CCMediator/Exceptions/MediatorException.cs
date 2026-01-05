namespace CCMediator;

/// <summary>
/// Represents errors that occur within the mediator infrastructure (e.g., handler resolution,
/// dispatch/pipeline construction). User handler exceptions are typically not wrapped.
/// </summary>
public sealed class MediatorException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public MediatorException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MediatorException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorException"/> class.
    /// </summary>
    public MediatorException()
    {
    }
}
