namespace CCMediator;
    
/// <summary>
/// Controls how exceptions from notification handlers are handled during sequential publishing.
/// </summary>
public enum NotificationPublishErrorHandling
{
    /// <summary>
    /// Stops when the first handler throws and propagates that exception.
    /// </summary>
    StopOnFirstException = 0,

    /// <summary>
    /// Continues executing remaining handlers and throws an <see cref="AggregateException"/> at the end.
    /// </summary>
    ContinueAndAggregateExceptions = 1
}
