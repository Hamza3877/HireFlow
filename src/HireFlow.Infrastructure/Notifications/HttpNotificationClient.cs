using System.Net.Http.Json;
using HireFlow.Application.Interfaces;
using HireFlow.Application.Notifications;
using Microsoft.Extensions.Logging;

namespace HireFlow.Infrastructure.Notifications;

// Sends the notification to the separate HireFlow.Notifications service over HTTP.
// If that service is down we only log a warning, so the hiring action itself still succeeds.
public class HttpNotificationClient : INotificationClient
{
    private readonly HttpClient _http;
    private readonly ILogger<HttpNotificationClient> _logger;

    public HttpNotificationClient(HttpClient http, ILogger<HttpNotificationClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task SendAsync(NotificationMessage message)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("notify", message);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Notification service answered {StatusCode}", (int)response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Notification service is not reachable. Continuing without it.");
        }
    }
}
