namespace SimpleMediator;

public sealed class SimpleMediatorOptions
{
    public NotificationPublishMode NotificationPublishMode { get; set; } = NotificationPublishMode.Parallel;

    public NotificationPublishErrorHandling SequentialPublishErrorHandling { get; set; } = NotificationPublishErrorHandling.StopOnFirstException;

    public bool AggregateExceptionsInParallel { get; set; } = true;
}
