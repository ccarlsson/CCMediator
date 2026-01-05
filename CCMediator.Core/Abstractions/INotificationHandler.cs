namespace CCMediator;

/// <summary>
/// Handles a notification.
/// </summary>
/// <typeparam name="TNotification">The notification type.</typeparam>
public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    /// <summary>
    /// Handles the given notification.
    /// </summary>
    /// <param name="notification">The notification instance.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that completes when handling has finished.</returns>
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}
