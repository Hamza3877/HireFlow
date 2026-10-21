using HireFlow.Application.Dtos;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;

namespace HireFlow.Application.Services;

// Writes and reads the audit trail (who did what, when).
public class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogRepository _logs;

    public ActivityLogService(IActivityLogRepository logs)
    {
        _logs = logs;
    }

    public async Task LogAsync(string action, string details, int? userId, int? applicationId = null)
    {
        var log = new ActivityLog
        {
            Action = action,
            Details = details,
            UserId = userId,
            ApplicationId = applicationId,
            CreatedAt = DateTime.UtcNow
        };
        await _logs.AddAsync(log);
    }

    public async Task<List<ActivityLogResponse>> GetRecentAsync(int count)
    {
        var safeCount = Math.Clamp(count, 1, 200);
        var logs = await _logs.GetRecentAsync(safeCount);
        return logs.Select(ToResponse).ToList();
    }

    public async Task<List<ActivityLogResponse>> GetForApplicationAsync(int applicationId)
    {
        var logs = await _logs.GetForApplicationAsync(applicationId);
        return logs.Select(ToResponse).ToList();
    }

    public static ActivityLogResponse ToResponse(ActivityLog log)
    {
        return new ActivityLogResponse
        {
            Id = log.Id,
            Action = log.Action,
            Details = log.Details,
            UserId = log.UserId,
            UserName = log.User?.FullName,
            ApplicationId = log.ApplicationId,
            CreatedAt = log.CreatedAt
        };
    }
}
