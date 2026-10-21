using HireFlow.Application.Common;
using HireFlow.Application.Dtos;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Interfaces;

public interface IJobService
{
    Task<PagedResult<JobResponse>> GetPagedAsync(JobListQuery query);
    Task<PagedResult<JobResponse>> GetOpenJobsAsync(JobListQuery query);
    Task<JobResponse> GetByIdAsync(int id);
    Task<JobResponse> GetOpenJobByIdAsync(int id);
    Task<JobResponse> CreateAsync(JobRequest request, int userId);
    Task<JobResponse> UpdateAsync(int id, JobRequest request, int userId);
    Task<JobResponse> SubmitForApprovalAsync(int id, int userId);
    Task<JobResponse> ApproveAsync(int id, int userId, Role userRole);
    Task<JobResponse> CloseAsync(int id, int userId);
}
