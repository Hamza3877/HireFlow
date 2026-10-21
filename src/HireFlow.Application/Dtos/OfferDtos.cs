using System.ComponentModel.DataAnnotations;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Dtos;

public class CreateOfferRequest
{
    [Required]
    public int ApplicationId { get; set; }

    [Range(1, 100000000)]
    public decimal Salary { get; set; }
}

// Seen by staff only. Contains the secret token used to build the candidate's link.
public class OfferResponse
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public OfferStatus Status { get; set; }
    public string AcceptToken { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Seen by the candidate who opens the secret link.
public class PublicOfferResponse
{
    public string CandidateName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public OfferStatus Status { get; set; }
}

public class OfferDecisionRequest
{
    public bool Accept { get; set; }
}

public class OnboardingTaskResponse
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; }
}
