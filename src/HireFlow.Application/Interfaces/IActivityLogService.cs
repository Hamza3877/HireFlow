using HireFlow.Application.Dtos;

namespace HireFlow.Application.Interfaces;

public interface IActivityLogService
{
    Task LogAsync(string action, string details, int? userId, int? applicationId = null);
    Task<List<ActivityLogResponse>> GetRecentAsync(int count);
    Task<List<ActivityLogResponse>> GetForApplicationAsync(int applicationId);
}
