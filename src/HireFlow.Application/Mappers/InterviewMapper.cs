using HireFlow.Application.Dtos;
using HireFlow.Domain.Entities;

namespace HireFlow.Application.Mappers;

// Turns an Interview (with application, candidate, interviewer and scorecard loaded) into a DTO.
public static class InterviewMapper
{
    public static InterviewResponse ToResponse(this Interview interview)
    {
        return new InterviewResponse
        {
            Id = interview.Id,
            ApplicationId = interview.ApplicationId,
            CandidateName = interview.Application.Candidate.FullName,
            InterviewerId = interview.InterviewerId,
            InterviewerName = interview.Interviewer.FullName,
            StartTime = interview.StartTime,
            EndTime = interview.EndTime,
            Status = interview.Status,
            Scorecard = interview.Scorecard == null ? null : new ScorecardResponse
            {
                Rating = interview.Scorecard.Rating,
                Comments = interview.Scorecard.Comments,
                Recommendation = interview.Scorecard.Recommendation
            }
        };
    }
}
