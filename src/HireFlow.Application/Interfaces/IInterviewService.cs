using HireFlow.Application.Dtos;

namespace HireFlow.Application.Interfaces;

public interface IInterviewService
{
    Task<List<InterviewResponse>> GetListAsync(int? applicationId, int? interviewerId);
    Task<InterviewResponse> ScheduleAsync(ScheduleInterviewRequest request, int userId);
    Task<InterviewResponse> RescheduleAsync(int id, RescheduleInterviewRequest request, int userId);
    Task<InterviewResponse> CancelAsync(int id, int userId);
    Task<InterviewResponse> SubmitScorecardAsync(int interviewId, ScorecardRequest request, int userId);
}
