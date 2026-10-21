using System.ComponentModel.DataAnnotations;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Dtos;

public class ApplyRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

// The uploaded CV, described without any ASP.NET types so the Application layer stays clean.
public class CvUpload
{
    public Stream Content { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Length { get; set; }
}

public class ApplicationResponse
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string CandidatePhone { get; set; } = string.Empty;
    public ApplicationStage Stage { get; set; }
    public int MatchScore { get; set; }
    public string ApplicationCode { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime StageChangedAt { get; set; }
    public int DaysInStage { get; set; }
}

public class ApplicationListQuery
{
    public string? Search { get; set; }
    public ApplicationStage? Stage { get; set; }
    public int? JobId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class MoveStageRequest
{
    [Required]
    public ApplicationStage Stage { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}

// What a candidate sees on the public status page. Nothing internal.
public class ApplicationStatusResponse
{
    public string JobTitle { get; set; } = string.Empty;
    public ApplicationStage Stage { get; set; }
    public DateTime AppliedAt { get; set; }
}

public class ActivityLogResponse
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public int? ApplicationId { get; set; }
    public DateTime CreatedAt { get; set; }
}
