using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Interfaces;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(int id);
    Task<(List<Job> Items, int TotalCount)> GetPagedAsync(string? search, JobStatus? status, int page, int pageSize);
    Task<int> CountByStatusAsync(JobStatus status);
    Task AddAsync(Job job);
    Task SaveChangesAsync();
}
