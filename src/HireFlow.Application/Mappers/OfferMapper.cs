using HireFlow.Application.Dtos;
using HireFlow.Domain.Entities;

namespace HireFlow.Application.Mappers;

// Turns offers and onboarding tasks into DTOs.
public static class OfferMapper
{
    public static OfferResponse ToResponse(this Offer offer)
    {
        return new OfferResponse
        {
            Id = offer.Id,
            ApplicationId = offer.ApplicationId,
            CandidateName = offer.Application.Candidate.FullName,
            JobTitle = offer.Application.Job.Title,
            Salary = offer.Salary,
            Status = offer.Status,
            AcceptToken = offer.AcceptToken,
            CreatedAt = offer.CreatedAt
        };
    }

    public static PublicOfferResponse ToPublicResponse(this Offer offer)
    {
        return new PublicOfferResponse
        {
            CandidateName = offer.Application.Candidate.FullName,
            JobTitle = offer.Application.Job.Title,
            Salary = offer.Salary,
            Status = offer.Status
        };
    }

    public static OnboardingTaskResponse ToResponse(this OnboardingTask task)
    {
        return new OnboardingTaskResponse
        {
            Id = task.Id,
            ApplicationId = task.ApplicationId,
            Title = task.Title,
            IsDone = task.IsDone
        };
    }
}
