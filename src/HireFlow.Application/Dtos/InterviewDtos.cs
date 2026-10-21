using System.ComponentModel.DataAnnotations;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Dtos;

public class ScheduleInterviewRequest
{
    [Required]
    public int ApplicationId { get; set; }

    [Required]
    public int InterviewerId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }
}

public class RescheduleInterviewRequest
{
    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }
}

public class ScorecardRequest
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required, MaxLength(2000)]
    public string Comments { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Recommendation { get; set; } = string.Empty;
}

public class ScorecardResponse
{
    public int Rating { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

public class InterviewResponse
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public int InterviewerId { get; set; }
    public string InterviewerName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public InterviewStatus Status { get; set; }
    public ScorecardResponse? Scorecard { get; set; }
}
