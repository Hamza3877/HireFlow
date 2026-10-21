using HireFlow.Application.Dtos;
using HireFlow.Application.Exceptions;
using HireFlow.Application.Interfaces;
using HireFlow.Application.Services;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using NSubstitute;

namespace HireFlow.Tests;

// Tests for offers, the single-use accept link and onboarding.
public class OfferServiceTests
{
    private readonly IOfferRepository _offers = Substitute.For<IOfferRepository>();
    private readonly IApplicationRepository _applications = Substitute.For<IApplicationRepository>();
    private readonly IJobRepository _jobs = Substitute.For<IJobRepository>();
    private readonly OfferService _service;

    public OfferServiceTests()
    {
        _service = new OfferService(_offers, _applications, _jobs,
            Substitute.For<IActivityLogService>(), Substitute.For<INotificationClient>());
    }

    private static JobApplication ApplicationAt(ApplicationStage stage) => new()
    {
        Id = 10,
        JobId = 1,
        Job = new Job { Id = 1, Title = "Dev", Positions = 1 },
        Candidate = new Candidate { FullName = "Ali Khan" },
        Stage = stage
    };

    private static Offer OfferWithStatus(OfferStatus status, ApplicationStage stage = ApplicationStage.Offer) => new()
    {
        Id = 1,
        ApplicationId = 10,
        Application = ApplicationAt(stage),
        Salary = 100000,
        Status = status,
        AcceptToken = "token"
    };

    [Fact]
    public async Task Create_WhenStageIsNotOffer_Throws()
    {
        _applications.GetByIdAsync(10).Returns(ApplicationAt(ApplicationStage.Interview));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.CreateAsync(new CreateOfferRequest { ApplicationId = 10, Salary = 1000 }, 2));
    }

    [Fact]
    public async Task Create_WhenStageIsOffer_CreatesPendingOfferWithToken()
    {
        _applications.GetByIdAsync(10).Returns(ApplicationAt(ApplicationStage.Offer));

        var result = await _service.CreateAsync(new CreateOfferRequest { ApplicationId = 10, Salary = 1000 }, 2);

        Assert.Equal(OfferStatus.Pending, result.Status);
        Assert.True(result.AcceptToken.Length >= 32);
    }

    [Fact]
    public async Task Approve_WhenRoleIsRecruiter_ThrowsForbidden()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() => _service.ApproveAsync(1, 2, Role.Recruiter));
    }

    [Fact]
    public async Task Approve_WhenOfferIsPending_Approves()
    {
        _offers.GetByIdAsync(1).Returns(OfferWithStatus(OfferStatus.Pending));

        var result = await _service.ApproveAsync(1, 3, Role.HiringManager);

        Assert.Equal(OfferStatus.Approved, result.Status);
    }

    [Fact]
    public async Task Respond_WhenOfferIsNotApproved_Throws()
    {
        _offers.GetByTokenAsync("token").Returns(OfferWithStatus(OfferStatus.Pending));

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RespondAsync("token", true));
    }

    [Fact]
    public async Task Respond_WhenLinkIsUsedTwice_ThrowsSecondTime()
    {
        var offer = OfferWithStatus(OfferStatus.Approved);
        _offers.GetByTokenAsync("token").Returns(offer);
        _jobs.GetByIdAsync(1).Returns(offer.Application.Job);

        await _service.RespondAsync("token", true);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RespondAsync("token", true));
    }

    [Fact]
    public async Task Respond_WhenAccepted_HiresCandidateAndCreatesOnboardingAndClosesJob()
    {
        var offer = OfferWithStatus(OfferStatus.Approved);
        _offers.GetByTokenAsync("token").Returns(offer);
        _jobs.GetByIdAsync(1).Returns(offer.Application.Job);
        _applications.CountHiredForJobAsync(1).Returns(0);

        await _service.RespondAsync("token", true);

        Assert.Equal(ApplicationStage.Hired, offer.Application.Stage);
        Assert.Equal(JobStatus.Closed, offer.Application.Job.Status);
        await _offers.Received(1).AddTasksAsync(Arg.Is<IEnumerable<OnboardingTask>>(tasks => tasks.Count() == 4));
    }

    [Fact]
    public async Task Respond_WhenAcceptedButPositionsRemain_KeepsJobOpen()
    {
        var offer = OfferWithStatus(OfferStatus.Approved);
        offer.Application.Job.Positions = 3;
        offer.Application.Job.Status = JobStatus.Open;
        _offers.GetByTokenAsync("token").Returns(offer);
        _jobs.GetByIdAsync(1).Returns(offer.Application.Job);
        _applications.CountHiredForJobAsync(1).Returns(0);

        await _service.RespondAsync("token", true);

        Assert.Equal(JobStatus.Open, offer.Application.Job.Status);
    }

    [Fact]
    public async Task Respond_WhenDeclined_RejectsApplication()
    {
        var offer = OfferWithStatus(OfferStatus.Approved);
        _offers.GetByTokenAsync("token").Returns(offer);

        await _service.RespondAsync("token", false);

        Assert.Equal(OfferStatus.Declined, offer.Status);
        Assert.Equal(ApplicationStage.Rejected, offer.Application.Stage);
    }
}
