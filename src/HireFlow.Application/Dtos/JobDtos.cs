using System.ComponentModel.DataAnnotations;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Dtos;

// Used for both create and update.
public class JobRequest
{
    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Location { get; set; } = string.Empty;

    [Required, MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    // Comma separated, e.g. "C#, SQL Server, Docker"
    [Required, MaxLength(500)]
    public string RequiredSkills { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int Positions { get; set; } = 1;
}

public class JobResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RequiredSkills { get; set; } = string.Empty;
    public int Positions { get; set; }
    public JobStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class JobListQuery
{
    public string? Search { get; set; }
    public JobStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
