using HireFlow.Application.Common;
using HireFlow.Application.Dtos;
using HireFlow.Application.Exceptions;
using HireFlow.Application.Interfaces;
using HireFlow.Application.Mappers;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Services;

// All business rules for jobs live here: edit, submit, approve, close.
public class JobService : IJobService
{
    private readonly IJobRepository _jobs;
    private readonly IActivityLogService _log;

    public JobService(IJobRepository jobs, IActivityLogService log)
    {
        _jobs = jobs;
        _log = log;
    }

    public async Task<PagedResult<JobResponse>> GetPagedAsync(JobListQuery query)
    {
        return await LoadPageAsync(query.Search, query.Status, query.Page, query.PageSize);
    }

    // For the public careers site: only Open jobs are ever shown.
    public async Task<PagedResult<JobResponse>> GetOpenJobsAsync(JobListQuery query)
    {
        return await LoadPageAsync(query.Search, JobStatus.Open, query.Page, query.PageSize);
    }

    public async Task<JobResponse> GetByIdAsync(int id)
    {
        var job = await FindJobAsync(id);
        return job.ToResponse();
    }

    public async Task<JobResponse> GetOpenJobByIdAsync(int id)
    {
        var job = await FindJobAsync(id);
        if (job.Status != JobStatus.Open)
        {
            throw new NotFoundException("Job not found.");
        }
        return job.ToResponse();
    }

    public async Task<JobResponse> CreateAsync(JobRequest request, int userId)
    {
        CheckPositions(request.Positions);

        var job = new Job
        {
            Title = request.Title.Trim(),
            Department = request.Department.Trim(),
            Location = request.Location.Trim(),
            Description = request.Description.Trim(),
            RequiredSkills = request.RequiredSkills.Trim(),
            Positions = request.Positions,
            Status = JobStatus.Draft,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _jobs.AddAsync(job);
        await _jobs.SaveChangesAsync();
        await _log.LogAsync("JobCreated", $"Job '{job.Title}' created as draft.", userId);
        return job.ToResponse();
    }

    public async Task<JobResponse> UpdateAsync(int id, JobRequest request, int userId)
    {
        var job = await FindJobAsync(id);

        if (job.Status != JobStatus.Draft)
        {
            throw new BusinessRuleException("Only draft jobs can be edited.");
        }
        CheckPositions(request.Positions);

        job.Title = request.Title.Trim();
        job.Department = request.Department.Trim();
        job.Location = request.Location.Trim();
        job.Description = request.Description.Trim();
        job.RequiredSkills = request.RequiredSkills.Trim();
        job.Positions = request.Positions;

        await _jobs.SaveChangesAsync();
        await _log.LogAsync("JobUpdated", $"Job '{job.Title}' updated.", userId);
        return job.ToResponse();
    }

    public async Task<JobResponse> SubmitForApprovalAsync(int id, int userId)
    {
        var job = await FindJobAsync(id);

        if (job.Status != JobStatus.Draft)
        {
            throw new BusinessRuleException("Only draft jobs can be submitted for approval.");
        }

        job.Status = JobStatus.PendingApproval;
        await _jobs.SaveChangesAsync();
        await _log.LogAsync("JobSubmitted", $"Job '{job.Title}' submitted for approval.", userId);
        return job.ToResponse();
    }

    public async Task<JobResponse> ApproveAsync(int id, int userId, Role userRole)
    {
        if (userRole != Role.HiringManager && userRole != Role.Admin)
        {
            throw new ForbiddenException("Only a hiring manager or admin can approve jobs.");
        }

        var job = await FindJobAsync(id);

        if (job.Status != JobStatus.PendingApproval)
        {
            throw new BusinessRuleException("Only jobs waiting for approval can be approved.");
        }

        job.Status = JobStatus.Open;
        await _jobs.SaveChangesAsync();
        await _log.LogAsync("JobApproved", $"Job '{job.Title}' approved and is now open.", userId);
        return job.ToResponse();
    }

    public async Task<JobResponse> CloseAsync(int id, int userId)
    {
        var job = await FindJobAsync(id);

        if (job.Status == JobStatus.Closed)
        {
            throw new BusinessRuleException("This job is already closed.");
        }

        job.Status = JobStatus.Closed;
        await _jobs.SaveChangesAsync();
        await _log.LogAsync("JobClosed", $"Job '{job.Title}' closed.", userId);
        return job.ToResponse();
    }

    private async Task<PagedResult<JobResponse>> LoadPageAsync(string? search, JobStatus? status, int page, int pageSize)
    {
        var safePage = Paging.SafePage(page);
        var safePageSize = Paging.SafePageSize(pageSize);

        var (items, total) = await _jobs.GetPagedAsync(search, status, safePage, safePageSize);

        return new PagedResult<JobResponse>
        {
            Items = items.Select(j => j.ToResponse()).ToList(),
            Page = safePage,
            PageSize = safePageSize,
            TotalCount = total
        };
    }

    private async Task<Job> FindJobAsync(int id)
    {
        var job = await _jobs.GetByIdAsync(id);
        return job ?? throw new NotFoundException("Job not found.");
    }

    private static void CheckPositions(int positions)
    {
        if (positions < 1)
        {
            throw new BusinessRuleException("A job must have at least 1 position.");
        }
    }
}
