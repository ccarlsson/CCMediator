namespace CCMediator;

/// <summary>
/// Handles a notification.
/// </summary>
public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}
