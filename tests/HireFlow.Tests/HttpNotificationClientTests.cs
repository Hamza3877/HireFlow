using HireFlow.Application.Notifications;
using HireFlow.Infrastructure.Notifications;
using Microsoft.Extensions.Logging.Abstractions;

namespace HireFlow.Tests;

// The main action must succeed even when the Notifications service is down.
public class HttpNotificationClientTests
{
    private class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Service is down");
        }
    }

    [Fact]
    public async Task Send_WhenServiceIsDown_DoesNotThrow()
    {
        var http = new HttpClient(new ThrowingHandler()) { BaseAddress = new Uri("http://localhost:9/") };
        var client = new HttpNotificationClient(http, NullLogger<HttpNotificationClient>.Instance);

        var exception = await Record.ExceptionAsync(() =>
            client.SendAsync(new NotificationMessage("Test", "Subject", "Body")));

        Assert.Null(exception);
    }
}
