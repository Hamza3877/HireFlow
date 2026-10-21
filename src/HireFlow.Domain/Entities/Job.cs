using HireFlow.Domain.Enums;

namespace HireFlow.Domain.Entities;

// A job opening. It starts as Draft and must be approved before candidates can apply.
public class Job
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    // Comma separated, for example "C#, SQL Server, Docker". Used for the match score.
    public string RequiredSkills { get; set; } = string.Empty;
    public int Positions { get; set; } = 1;
    public JobStatus Status { get; set; } = JobStatus.Draft;
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
