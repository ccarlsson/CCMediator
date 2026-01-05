namespace CCMediator;

/// <summary>
/// Options controlling mediator behavior.
/// </summary>
public sealed class SimpleMediatorOptions
{
    /// <summary>
    /// Gets or sets how notifications are published.
    /// </summary>
    public NotificationPublishMode NotificationPublishMode { get; set; } = NotificationPublishMode.Parallel;

    /// <summary>
    /// Gets or sets how exceptions are handled when <see cref="NotificationPublishMode.Sequential"/> is used.
    /// </summary>
    public NotificationPublishErrorHandling SequentialPublishErrorHandling { get; set; } = NotificationPublishErrorHandling.StopOnFirstException;

    /// <summary>
    /// Gets or sets whether exceptions should be aggregated (as <see cref="AggregateException"/>) when publishing in parallel.
    /// </summary>
    public bool AggregateExceptionsInParallel { get; set; } = true;
}
