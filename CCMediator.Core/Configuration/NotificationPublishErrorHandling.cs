namespace CCMediator;

/// <summary>
/// Controls how exceptions from notification handlers are handled during sequential publishing.
/// </summary>
public enum NotificationPublishErrorHandling
{
    StopOnFirstException = 0,
    ContinueAndAggregateExceptions = 1
}
