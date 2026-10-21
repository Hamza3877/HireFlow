using HireFlow.Application.Dtos;
using HireFlow.Application.Exceptions;
using HireFlow.Application.Interfaces;
using HireFlow.Application.Mappers;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Services;

// Business rules for interviews and scorecards.
public class InterviewService : IInterviewService
{
    private readonly IInterviewRepository _interviews;
    private readonly IApplicationRepository _applications;
    private readonly IUserRepository _users;
    private readonly IActivityLogService _log;

    public InterviewService(
        IInterviewRepository interviews,
        IApplicationRepository applications,
        IUserRepository users,
        IActivityLogService log)
    {
        _interviews = interviews;
        _applications = applications;
        _users = users;
        _log = log;
    }

    public async Task<List<InterviewResponse>> GetListAsync(int? applicationId, int? interviewerId)
    {
        var interviews = await _interviews.GetListAsync(applicationId, interviewerId);
        return interviews.Select(i => i.ToResponse()).ToList();
    }

    public async Task<InterviewResponse> ScheduleAsync(ScheduleInterviewRequest request, int userId)
    {
        CheckTimes(request.StartTime, request.EndTime);

        var application = await _applications.GetByIdAsync(request.ApplicationId)
            ?? throw new NotFoundException("Application not found.");

        if (application.Stage == ApplicationStage.Rejected || application.Stage == ApplicationStage.Hired)
        {
            throw new BusinessRuleException("Interviews cannot be scheduled for rejected or hired candidates.");
        }

        var interviewer = await _users.GetByIdAsync(request.InterviewerId)
            ?? throw new NotFoundException("Interviewer not found.");

        if (await _interviews.HasOverlapAsync(interviewer.Id, request.StartTime, request.EndTime, null))
        {
            throw new BusinessRuleException("The interviewer already has an interview at that time.");
        }

        var interview = new Interview
        {
            ApplicationId = application.Id,
            Application = application,
            InterviewerId = interviewer.Id,
            Interviewer = interviewer,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = InterviewStatus.Scheduled
        };

        await _interviews.AddAsync(interview);
        await _interviews.SaveChangesAsync();
        await _log.LogAsync(
            "InterviewScheduled",
            $"Interview with {application.Candidate.FullName} scheduled for {request.StartTime:yyyy-MM-dd HH:mm}.",
            userId,
            application.Id);

        return interview.ToResponse();
    }

    public async Task<InterviewResponse> RescheduleAsync(int id, RescheduleInterviewRequest request, int userId)
    {
        CheckTimes(request.StartTime, request.EndTime);
        var interview = await FindInterviewAsync(id);
        CheckCanChange(interview);

        if (await _interviews.HasOverlapAsync(interview.InterviewerId, request.StartTime, request.EndTime, interview.Id))
        {
            throw new BusinessRuleException("The interviewer already has an interview at that time.");
        }

        interview.StartTime = request.StartTime;
        interview.EndTime = request.EndTime;
        await _interviews.SaveChangesAsync();
        await _log.LogAsync(
            "InterviewRescheduled",
            $"Interview with {interview.Application.Candidate.FullName} moved to {request.StartTime:yyyy-MM-dd HH:mm}.",
            userId,
            interview.ApplicationId);

        return interview.ToResponse();
    }

    public async Task<InterviewResponse> CancelAsync(int id, int userId)
    {
        var interview = await FindInterviewAsync(id);
        CheckCanChange(interview);

        interview.Status = InterviewStatus.Cancelled;
        await _interviews.SaveChangesAsync();
        await _log.LogAsync(
            "InterviewCancelled",
            $"Interview with {interview.Application.Candidate.FullName} cancelled.",
            userId,
            interview.ApplicationId);

        return interview.ToResponse();
    }

    public async Task<InterviewResponse> SubmitScorecardAsync(int interviewId, ScorecardRequest request, int userId)
    {
        var interview = await FindInterviewAsync(interviewId);

        if (interview.InterviewerId != userId)
        {
            throw new ForbiddenException("Only the assigned interviewer can submit the scorecard.");
        }
        if (interview.Status == InterviewStatus.Cancelled)
        {
            throw new BusinessRuleException("A cancelled interview cannot have a scorecard.");
        }
        if (interview.Scorecard != null)
        {
            throw new BusinessRuleException("A scorecard was already submitted for this interview.");
        }
        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new BusinessRuleException("Rating must be between 1 and 5.");
        }

        var scorecard = new Scorecard
        {
            InterviewId = interview.Id,
            Rating = request.Rating,
            Comments = request.Comments.Trim(),
            Recommendation = request.Recommendation.Trim()
        };

        await _interviews.AddScorecardAsync(scorecard);
        interview.Scorecard = scorecard;
        interview.Status = InterviewStatus.Completed;
        await _interviews.SaveChangesAsync();
        await _log.LogAsync(
            "ScorecardSubmitted",
            $"Scorecard ({request.Rating}/5) submitted for {interview.Application.Candidate.FullName}.",
            userId,
            interview.ApplicationId);

        return interview.ToResponse();
    }

    private async Task<Interview> FindInterviewAsync(int id)
    {
        var interview = await _interviews.GetByIdAsync(id);
        return interview ?? throw new NotFoundException("Interview not found.");
    }

    private static void CheckTimes(DateTime start, DateTime end)
    {
        if (end <= start)
        {
            throw new BusinessRuleException("The end time must be after the start time.");
        }
    }

    // Reschedule and cancel are only allowed for scheduled interviews that have not started yet.
    private static void CheckCanChange(Interview interview)
    {
        if (interview.Status != InterviewStatus.Scheduled)
        {
            throw new BusinessRuleException("Only scheduled interviews can be changed.");
        }
        if (interview.StartTime <= DateTime.UtcNow)
        {
            throw new BusinessRuleException("An interview that has already started cannot be changed.");
        }
    }
}
