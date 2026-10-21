using HireFlow.Application.Dtos;
using HireFlow.Application.Exceptions;
using HireFlow.Application.Interfaces;
using HireFlow.Application.Services;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using NSubstitute;

namespace HireFlow.Tests;

// Tests for the job business rules. The repository is faked with NSubstitute (no real database).
public class JobServiceTests
{
    private readonly IJobRepository _repo = Substitute.For<IJobRepository>();
    private readonly IActivityLogService _log = Substitute.For<IActivityLogService>();
    private readonly JobService _service;

    public JobServiceTests()
    {
        _service = new JobService(_repo, _log);
    }

    private static Job JobWithStatus(JobStatus status) => new() { Id = 1, Title = "Dev", Status = status, Positions = 1 };

    private static JobRequest ValidRequest() => new()
    {
        Title = "Dev", Department = "Eng", Location = "Karachi",
        Description = "Build things", RequiredSkills = "C#", Positions = 1
    };

    [Fact]
    public async Task Create_CreatesDraftJob()
    {
        var result = await _service.CreateAsync(ValidRequest(), 1);

        Assert.Equal(JobStatus.Draft, result.Status);
        await _repo.Received(1).AddAsync(Arg.Any<Job>());
    }

    [Fact]
    public async Task Create_WhenPositionsIsZero_Throws()
    {
        var request = ValidRequest();
        request.Positions = 0;

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateAsync(request, 1));
    }

    [Fact]
    public async Task Update_WhenJobIsOpen_Throws()
    {
        _repo.GetByIdAsync(1).Returns(JobWithStatus(JobStatus.Open));

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.UpdateAsync(1, ValidRequest(), 1));
    }

    [Fact]
    public async Task Update_WhenJobIsDraft_Works()
    {
        _repo.GetByIdAsync(1).Returns(JobWithStatus(JobStatus.Draft));

        var result = await _service.UpdateAsync(1, ValidRequest(), 1);

        Assert.Equal("Dev", result.Title);
    }

    [Fact]
    public async Task Submit_WhenJobIsDraft_MovesToPendingApproval()
    {
        _repo.GetByIdAsync(1).Returns(JobWithStatus(JobStatus.Draft));

        var result = await _service.SubmitForApprovalAsync(1, 1);

        Assert.Equal(JobStatus.PendingApproval, result.Status);
    }

    [Fact]
    public async Task Submit_WhenJobIsOpen_Throws()
    {
        _repo.GetByIdAsync(1).Returns(JobWithStatus(JobStatus.Open));

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.SubmitForApprovalAsync(1, 1));
    }

    [Fact]
    public async Task Approve_WhenJobIsDraft_Throws()
    {
        _repo.GetByIdAsync(1).Returns(JobWithStatus(JobStatus.Draft));

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ApproveAsync(1, 3, Role.HiringManager));
    }

    [Fact]
    public async Task Approve_WhenRoleIsRecruiter_Throws()
    {
        _repo.GetByIdAsync(1).Returns(JobWithStatus(JobStatus.PendingApproval));

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.ApproveAsync(1, 2, Role.Recruiter));
    }

    [Fact]
    public async Task Approve_WhenPendingAndManager_OpensJob()
    {
        _repo.GetByIdAsync(1).Returns(JobWithStatus(JobStatus.PendingApproval));

        var result = await _service.ApproveAsync(1, 3, Role.HiringManager);

        Assert.Equal(JobStatus.Open, result.Status);
    }

    [Fact]
    public async Task GetById_WhenMissing_ThrowsNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Job?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(99));
    }

    [Fact]
    public async Task GetOpenJobById_WhenJobIsDraft_ThrowsNotFound()
    {
        _repo.GetByIdAsync(1).Returns(JobWithStatus(JobStatus.Draft));

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetOpenJobByIdAsync(1));
    }
}
