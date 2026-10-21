using HireFlow.Domain.Enums;

namespace HireFlow.Domain.Entities;

// A scheduled interview between one staff member and one application.
public class Interview
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public JobApplication Application { get; set; } = null!;
    public int InterviewerId { get; set; }
    public User Interviewer { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;
    public Scorecard? Scorecard { get; set; }
}
