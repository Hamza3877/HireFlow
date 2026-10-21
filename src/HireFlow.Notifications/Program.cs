using System.Collections.Concurrent;

// A tiny separate service. The main API calls POST /notify when something happens
// (stage changed, offer approved...). It keeps the last messages in memory and writes
// every message to logs/notifications.log. Real email sending (SMTP) can be added here later
// without touching the main API: that is the point of having a separate service.

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var messages = new ConcurrentQueue<StoredNotification>();
var logFolder = Path.Combine(AppContext.BaseDirectory, "logs");
Directory.CreateDirectory(logFolder);
var logFile = Path.Combine(logFolder, "notifications.log");
var fileLock = new object();

app.MapPost("/notify", (NotifyRequest request, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(request.Type) || string.IsNullOrWhiteSpace(request.Subject))
    {
        return Results.BadRequest("Type and Subject are required.");
    }

    var stored = new StoredNotification(DateTime.UtcNow, request.Type, request.Subject, request.Body);
    messages.Enqueue(stored);
    while (messages.Count > 100)
    {
        messages.TryDequeue(out _);
    }

    lock (fileLock)
    {
        File.AppendAllText(logFile, $"{stored.At:u} [{stored.Type}] {stored.Subject} - {stored.Body}{Environment.NewLine}");
    }

    logger.LogInformation("Notification stored: {Type} - {Subject}", stored.Type, stored.Subject);
    return Results.Ok();
});

app.MapGet("/notifications", () => messages.Reverse());
app.MapGet("/health", () => Results.Ok("healthy"));

app.Run();

public record NotifyRequest(string Type, string Subject, string Body);

public record StoredNotification(DateTime At, string Type, string Subject, string Body);
