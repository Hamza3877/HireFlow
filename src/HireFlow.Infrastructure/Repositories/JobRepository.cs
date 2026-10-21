using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Infrastructure.Repositories;

// Database access for jobs.
public class JobRepository : IJobRepository
{
    private readonly AppDbContext _db;

    public JobRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Job?> GetByIdAsync(int id)
    {
        return await _db.Jobs.FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<(List<Job> Items, int TotalCount)> GetPagedAsync(string? search, JobStatus? status, int page, int pageSize)
    {
        var query = _db.Jobs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var text = search.Trim();
            query = query.Where(j => j.Title.Contains(text) || j.Department.Contains(text) || j.Location.Contains(text));
        }
        if (status.HasValue)
        {
            query = query.Where(j => j.Status == status.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<int> CountByStatusAsync(JobStatus status)
    {
        return await _db.Jobs.CountAsync(j => j.Status == status);
    }

    public async Task AddAsync(Job job)
    {
        await _db.Jobs.AddAsync(job);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
