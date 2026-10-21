using HireFlow.Domain.Entities;

namespace HireFlow.Application.Interfaces;

public interface IActivityLogRepository
{
    Task AddAsync(ActivityLog log);
    Task<List<ActivityLog>> GetRecentAsync(int count);
    Task<List<ActivityLog>> GetForApplicationAsync(int applicationId);
}
