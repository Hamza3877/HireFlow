namespace HireFlow.Domain.Entities;

// The interviewer's rating and feedback after an interview. One per interview.
public class Scorecard
{
    public int Id { get; set; }
    public int InterviewId { get; set; }
    public Interview Interview { get; set; } = null!;
    public int Rating { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}
