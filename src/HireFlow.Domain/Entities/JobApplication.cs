using HireFlow.Domain.Enums;

namespace HireFlow.Domain.Entities;

// One candidate applying for one job. The Stage tells where they are in the hiring pipeline.
public class JobApplication
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public Job Job { get; set; } = null!;
    public int CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
    public ApplicationStage Stage { get; set; } = ApplicationStage.Applied;
    public int MatchScore { get; set; }
    public string CvFilePath { get; set; } = string.Empty;
    public string CvText { get; set; } = string.Empty;
    // Short code the candidate uses (with email) to check their status.
    public string ApplicationCode { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime StageChangedAt { get; set; }
}
