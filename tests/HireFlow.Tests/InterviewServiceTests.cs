using HireFlow.Application.Dtos;
using HireFlow.Application.Exceptions;
using HireFlow.Application.Interfaces;
using HireFlow.Application.Services;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using NSubstitute;

namespace HireFlow.Tests;

// Tests for interview scheduling and scorecard rules.
public class InterviewServiceTests
{
    private readonly IInterviewRepository _interviews = Substitute.For<IInterviewRepository>();
    private readonly IApplicationRepository _applications = Substitute.For<IApplicationRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly InterviewService _service;

    public InterviewServiceTests()
    {
        _service = new InterviewService(_interviews, _applications, _users, Substitute.For<IActivityLogService>());
    }

    private static JobApplication ApplicationAt(ApplicationStage stage) => new()
    {
        Id = 10,
        Job = new Job { Id = 1, Title = "Dev" },
        Candidate = new Candidate { FullName = "Ali Khan" },
        Stage = stage
    };

    private static Interview InterviewStartingIn(TimeSpan fromNow, InterviewStatus status = InterviewStatus.Scheduled) => new()
    {
        Id = 1,
        ApplicationId = 10,
        InterviewerId = 2,
        Application = ApplicationAt(ApplicationStage.Interview),
        Interviewer = new User { Id = 2, FullName = "Rida" },
        StartTime = DateTime.UtcNow + fromNow,
        EndTime = DateTime.UtcNow + fromNow + TimeSpan.FromHours(1),
        Status = status
    };

    private ScheduleInterviewRequest ValidSchedule()
    {
        var start = DateTime.UtcNow.AddDays(2);
        return new ScheduleInterviewRequest { ApplicationId = 10, InterviewerId = 2, StartTime = start, EndTime = start.AddHours(1) };
    }

    private void SetUpValidSchedule(ApplicationStage stage = ApplicationStage.Screening)
    {
        _applications.GetByIdAsync(10).Returns(ApplicationAt(stage));
        _users.GetByIdAsync(2).Returns(new User { Id = 2, FullName = "Rida" });
    }

    [Fact]
    public async Task Schedule_WhenEndIsBeforeStart_Throws()
    {
        var request = ValidSchedule();
        request.EndTime = request.StartTime.AddMinutes(-5);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ScheduleAsync(request, 1));
    }

    [Fact]
    public async Task Schedule_WhenInterviewerHasOverlap_Throws()
    {
        SetUpValidSchedule();
        _interviews.HasOverlapAsync(2, Arg.Any<DateTime>(), Arg.Any<DateTime>(), null).Returns(true);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ScheduleAsync(ValidSchedule(), 1));
    }

    [Fact]
    public async Task Schedule_WhenCandidateIsRejected_Throws()
    {
        SetUpValidSchedule(ApplicationStage.Rejected);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ScheduleAsync(ValidSchedule(), 1));
    }

    [Fact]
    public async Task Schedule_WhenValid_CreatesInterview()
    {
        SetUpValidSchedule();

        var result = await _service.ScheduleAsync(ValidSchedule(), 1);

        Assert.Equal(InterviewStatus.Scheduled, result.Status);
        await _interviews.Received(1).AddAsync(Arg.Any<Interview>());
    }

    [Fact]
    public async Task Cancel_WhenInterviewAlreadyStarted_Throws()
    {
        _interviews.GetByIdAsync(1).Returns(InterviewStartingIn(TimeSpan.FromHours(-2)));

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CancelAsync(1, 1));
    }

    [Fact]
    public async Task Cancel_WhenInterviewIsInTheFuture_Works()
    {
        _interviews.GetByIdAsync(1).Returns(InterviewStartingIn(TimeSpan.FromDays(1)));

        var result = await _service.CancelAsync(1, 1);

        Assert.Equal(InterviewStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task Scorecard_WhenUserIsNotTheInterviewer_ThrowsForbidden()
    {
        _interviews.GetByIdAsync(1).Returns(InterviewStartingIn(TimeSpan.FromHours(-1)));

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.SubmitScorecardAsync(1, new ScorecardRequest { Rating = 4, Comments = "ok", Recommendation = "Hire" }, 99));
    }

    [Fact]
    public async Task Scorecard_WhenAlreadySubmitted_Throws()
    {
        var interview = InterviewStartingIn(TimeSpan.FromHours(-1));
        interview.Scorecard = new Scorecard { Rating = 3 };
        _interviews.GetByIdAsync(1).Returns(interview);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.SubmitScorecardAsync(1, new ScorecardRequest { Rating = 4, Comments = "ok", Recommendation = "Hire" }, 2));
    }

    [Fact]
    public async Task Scorecard_WhenRatingIsOutOfRange_Throws()
    {
        _interviews.GetByIdAsync(1).Returns(InterviewStartingIn(TimeSpan.FromHours(-1)));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.SubmitScorecardAsync(1, new ScorecardRequest { Rating = 9, Comments = "ok", Recommendation = "Hire" }, 2));
    }

    [Fact]
    public async Task Scorecard_WhenValid_CompletesInterview()
    {
        _interviews.GetByIdAsync(1).Returns(InterviewStartingIn(TimeSpan.FromHours(-1)));

        var result = await _service.SubmitScorecardAsync(1, new ScorecardRequest { Rating = 5, Comments = "Great", Recommendation = "Hire" }, 2);

        Assert.Equal(InterviewStatus.Completed, result.Status);
        Assert.Equal(5, result.Scorecard!.Rating);
    }
}
