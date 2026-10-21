namespace HireFlow.Application.Notifications;

// A small message sent to the Notifications service (for example "stage changed").
public class NotificationMessage
{
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public NotificationMessage() { }

    public NotificationMessage(string type, string subject, string body)
    {
        Type = type;
        Subject = subject;
        Body = body;
    }
}
