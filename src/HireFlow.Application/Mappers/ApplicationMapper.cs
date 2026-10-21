using HireFlow.Application.Dtos;
using HireFlow.Domain.Entities;

namespace HireFlow.Application.Mappers;

// Turns a JobApplication (with its Job and Candidate loaded) into a DTO.
public static class ApplicationMapper
{
    public static ApplicationResponse ToResponse(this JobApplication application)
    {
        return new ApplicationResponse
        {
            Id = application.Id,
            JobId = application.JobId,
            JobTitle = application.Job.Title,
            CandidateId = application.CandidateId,
            CandidateName = application.Candidate.FullName,
            CandidateEmail = application.Candidate.Email,
            CandidatePhone = application.Candidate.Phone,
            Stage = application.Stage,
            MatchScore = application.MatchScore,
            ApplicationCode = application.ApplicationCode,
            RejectionReason = application.RejectionReason,
            AppliedAt = application.AppliedAt,
            StageChangedAt = application.StageChangedAt,
            DaysInStage = (int)(DateTime.UtcNow - application.StageChangedAt).TotalDays
        };
    }
}
