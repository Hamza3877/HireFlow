using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Interfaces;

public interface IApplicationRepository
{
    // Loads the application together with its job and candidate.
    Task<JobApplication?> GetByIdAsync(int id);
    Task<(List<JobApplication> Items, int TotalCount)> GetPagedAsync(string? search, ApplicationStage? stage, int? jobId, int page, int pageSize);
    Task<bool> ExistsAsync(int jobId, string candidateEmail);
    Task<JobApplication?> GetByEmailAndCodeAsync(string email, string code);
    Task<bool> HasSubmittedScorecardAsync(int applicationId);
    Task<int> CountHiredForJobAsync(int jobId);
    Task<int> CountAllAsync();
    Task<Dictionary<ApplicationStage, int>> CountByStageAsync();
    Task<List<(DateTime AppliedAt, DateTime HiredAt)>> GetHiredDatesAsync();
    Task AddAsync(JobApplication application);
    Task SaveChangesAsync();
}
