using HireFlow.Application.Notifications;

namespace HireFlow.Application.Interfaces;

// Sends a notification. Implementations must never throw: a failed email must not break hiring actions.
public interface INotificationClient
{
    Task SendAsync(NotificationMessage message);
}
