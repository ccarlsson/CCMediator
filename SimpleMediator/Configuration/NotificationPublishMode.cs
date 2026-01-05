namespace SimpleMediator;

/// <summary>
/// Controls how notification handlers are executed.
/// </summary>
public enum NotificationPublishMode
{
    /// <summary>
    /// Executes notification handlers concurrently.
    /// </summary>
    Parallel = 0,

    /// <summary>
    /// Executes notification handlers one-by-one in registration order.
    /// </summary>
    Sequential = 1
}
