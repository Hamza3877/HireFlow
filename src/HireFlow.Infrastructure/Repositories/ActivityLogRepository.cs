using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Infrastructure.Repositories;

// Database access for the activity log. Each log entry is saved immediately.
public class ActivityLogRepository : IActivityLogRepository
{
    private readonly AppDbContext _db;

    public ActivityLogRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ActivityLog log)
    {
        await _db.ActivityLogs.AddAsync(log);
        await _db.SaveChangesAsync();
    }

    public async Task<List<ActivityLog>> GetRecentAsync(int count)
    {
        return await _db.ActivityLogs
            .AsNoTracking()
            .Include(l => l.User)
            .OrderByDescending(l => l.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<ActivityLog>> GetForApplicationAsync(int applicationId)
    {
        return await _db.ActivityLogs
            .AsNoTracking()
            .Include(l => l.User)
            .Where(l => l.ApplicationId == applicationId)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync();
    }
}
