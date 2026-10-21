using HireFlow.Application.Dtos;
using HireFlow.Application.Exceptions;
using HireFlow.Application.Interfaces;
using HireFlow.Application.Notifications;
using HireFlow.Application.Services;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using NSubstitute;

namespace HireFlow.Tests;

// Tests for applying and for the pipeline stage rules.
public class ApplicationServiceTests
{
    private readonly IApplicationRepository _applications = Substitute.For<IApplicationRepository>();
    private readonly ICandidateRepository _candidates = Substitute.For<ICandidateRepository>();
    private readonly IJobRepository _jobs = Substitute.For<IJobRepository>();
    private readonly ICvStorage _storage = Substitute.For<ICvStorage>();
    private readonly ICvTextExtractor _extractor = Substitute.For<ICvTextExtractor>();
    private readonly INotificationClient _notifier = Substitute.For<INotificationClient>();
    private readonly ApplicationService _service;

    public ApplicationServiceTests()
    {
        _storage.SaveAsync(Arg.Any<CvUpload>()).Returns("file.pdf");
        _storage.Open(Arg.Any<string>()).Returns(new MemoryStream());
        _extractor.Extract(Arg.Any<Stream>(), Arg.Any<string>()).Returns("C# and SQL Server");

        _service = new ApplicationService(
            _applications, _candidates, _jobs, _storage, _extractor,
            new MatchScoreCalculator(), Substitute.For<IActivityLogService>(), _notifier);
    }

    private static JobApplication ApplicationAt(ApplicationStage stage) => new()
    {
        Id = 10,
        JobId = 1,
        Job = new Job { Id = 1, Title = "Dev" },
        Candidate = new Candidate { Id = 5, FullName = "Ali Khan", Email = "ali@example.com" },
        Stage = stage,
        StageChangedAt = DateTime.UtcNow
    };

    private static ApplyRequest NewApplyRequest() => new() { FullName = "Ali Khan", Email = "ali@example.com", Phone = "0300" };

    private static CvUpload NewCv() => new() { FileName = "cv.pdf", ContentType = "application/pdf", Length = 100 };

    // ---------- Apply ----------

    [Fact]
    public async Task Apply_WhenJobIsClosed_Throws()
    {
        _jobs.GetByIdAsync(1).Returns(new Job { Id = 1, Status = JobStatus.Closed });

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ApplyAsync(1, NewApplyRequest(), NewCv()));
    }

    [Fact]
    public async Task Apply_WhenAlreadyApplied_Throws()
    {
        _jobs.GetByIdAsync(1).Returns(new Job { Id = 1, Status = JobStatus.Open });
        _applications.ExistsAsync(1, "ali@example.com").Returns(true);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ApplyAsync(1, NewApplyRequest(), NewCv()));
    }

    [Fact]
    public async Task Apply_WhenValid_CalculatesMatchScore()
    {
        _jobs.GetByIdAsync(1).Returns(new Job { Id = 1, Title = "Dev", Status = JobStatus.Open, RequiredSkills = "C#, SQL Server, Docker, Git" });

        var result = await _service.ApplyAsync(1, NewApplyRequest(), NewCv());

        Assert.Equal(50, result.MatchScore);
        Assert.Equal(ApplicationStage.Applied, result.Stage);
        Assert.False(string.IsNullOrEmpty(result.ApplicationCode));
    }

    // ---------- Stage rules ----------

    [Fact]
    public async Task MoveStage_ToOfferWithoutScorecard_Throws()
    {
        _applications.GetByIdAsync(10).Returns(ApplicationAt(ApplicationStage.Interview));
        _applications.HasSubmittedScorecardAsync(10).Returns(false);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.MoveStageAsync(10, new MoveStageRequest { Stage = ApplicationStage.Offer }, 2, Role.Recruiter));
    }

    [Fact]
    public async Task MoveStage_ToOfferWithScorecard_Works()
    {
        _applications.GetByIdAsync(10).Returns(ApplicationAt(ApplicationStage.Interview));
        _applications.HasSubmittedScorecardAsync(10).Returns(true);

        var result = await _service.MoveStageAsync(10, new MoveStageRequest { Stage = ApplicationStage.Offer }, 2, Role.Recruiter);

        Assert.Equal(ApplicationStage.Offer, result.Stage);
    }

    [Fact]
    public async Task MoveStage_ToRejectedWithoutReason_Throws()
    {
        _applications.GetByIdAsync(10).Returns(ApplicationAt(ApplicationStage.Screening));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.MoveStageAsync(10, new MoveStageRequest { Stage = ApplicationStage.Rejected }, 2, Role.Recruiter));
    }

    [Fact]
    public async Task MoveStage_ToRejectedWithReason_StoresReason()
    {
        _applications.GetByIdAsync(10).Returns(ApplicationAt(ApplicationStage.Screening));

        var result = await _service.MoveStageAsync(10,
            new MoveStageRequest { Stage = ApplicationStage.Rejected, Reason = "Not enough experience" }, 2, Role.Recruiter);

        Assert.Equal("Not enough experience", result.RejectionReason);
    }

    [Fact]
    public async Task MoveStage_WhenCandidateIsHired_Throws()
    {
        _applications.GetByIdAsync(10).Returns(ApplicationAt(ApplicationStage.Hired));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.MoveStageAsync(10, new MoveStageRequest { Stage = ApplicationStage.Screening }, 2, Role.Recruiter));
    }

    [Fact]
    public async Task MoveStage_WhenRoleIsHiringManager_ThrowsForbidden()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.MoveStageAsync(10, new MoveStageRequest { Stage = ApplicationStage.Screening }, 3, Role.HiringManager));
    }

    [Fact]
    public async Task MoveStage_DirectlyToHired_Throws()
    {
        _applications.GetByIdAsync(10).Returns(ApplicationAt(ApplicationStage.Offer));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.MoveStageAsync(10, new MoveStageRequest { Stage = ApplicationStage.Hired }, 2, Role.Recruiter));
    }

    [Fact]
    public async Task GetStatus_WhenCodeIsWrong_ThrowsNotFound()
    {
        _applications.GetByEmailAndCodeAsync(Arg.Any<string>(), Arg.Any<string>()).Returns((JobApplication?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetStatusAsync("a@b.com", "WRONG"));
    }
}
