using HireFlow.Application.Interfaces;
using HireFlow.Application.Notifications;
using Microsoft.Extensions.Logging;

namespace HireFlow.Infrastructure.Notifications;

// Used in Demo mode (free hosting runs one process only). It just writes the message to the log.
public class InProcessNotificationClient : INotificationClient
{
    private readonly ILogger<InProcessNotificationClient> _logger;

    public InProcessNotificationClient(ILogger<InProcessNotificationClient> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(NotificationMessage message)
    {
        _logger.LogInformation("Notification [{Type}] {Subject}", message.Type, message.Subject);
        return Task.CompletedTask;
    }
}
