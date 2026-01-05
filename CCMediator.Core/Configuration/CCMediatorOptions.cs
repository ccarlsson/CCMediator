namespace CCMediator;

/// <summary>
/// Options controlling mediator behavior.
/// </summary>
public sealed class CCMediatorOptions
{
    public NotificationPublishMode NotificationPublishMode { get; set; } = NotificationPublishMode.Parallel;

    public NotificationPublishErrorHandling SequentialPublishErrorHandling { get; set; } = NotificationPublishErrorHandling.StopOnFirstException;

    public bool AggregateExceptionsInParallel { get; set; } = true;
}
