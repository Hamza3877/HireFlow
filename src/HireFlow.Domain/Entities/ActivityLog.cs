namespace HireFlow.Domain.Entities;

// Audit trail: who did what and when. Used for the admin log and the candidate timeline.
public class ActivityLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public User? User { get; set; }
    public int? ApplicationId { get; set; }
    public DateTime CreatedAt { get; set; }
}
