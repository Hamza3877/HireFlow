using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Infrastructure.Repositories;

// Database access for job applications.
public class ApplicationRepository : IApplicationRepository
{
    private readonly AppDbContext _db;

    public ApplicationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<JobApplication?> GetByIdAsync(int id)
    {
        return await _db.Applications
            .Include(a => a.Job)
            .Include(a => a.Candidate)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<(List<JobApplication> Items, int TotalCount)> GetPagedAsync(
        string? search, ApplicationStage? stage, int? jobId, int page, int pageSize)
    {
        var query = _db.Applications.AsNoTracking().Include(a => a.Job).Include(a => a.Candidate).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var text = search.Trim();
            query = query.Where(a => a.Candidate.FullName.Contains(text) || a.Candidate.Email.Contains(text));
        }
        if (stage.HasValue)
        {
            query = query.Where(a => a.Stage == stage.Value);
        }
        if (jobId.HasValue)
        {
            query = query.Where(a => a.JobId == jobId.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<bool> ExistsAsync(int jobId, string candidateEmail)
    {
        return await _db.Applications.AnyAsync(a => a.JobId == jobId && a.Candidate.Email == candidateEmail);
    }

    public async Task<JobApplication?> GetByEmailAndCodeAsync(string email, string code)
    {
        return await _db.Applications
            .AsNoTracking()
            .Include(a => a.Job)
            .Include(a => a.Candidate)
            .FirstOrDefaultAsync(a => a.Candidate.Email == email && a.ApplicationCode == code);
    }

    public async Task<bool> HasSubmittedScorecardAsync(int applicationId)
    {
        return await _db.Scorecards.AnyAsync(s => s.Interview.ApplicationId == applicationId);
    }

    public async Task<int> CountHiredForJobAsync(int jobId)
    {
        return await _db.Applications.CountAsync(a => a.JobId == jobId && a.Stage == ApplicationStage.Hired);
    }

    public async Task<int> CountAllAsync()
    {
        return await _db.Applications.CountAsync();
    }

    public async Task<Dictionary<ApplicationStage, int>> CountByStageAsync()
    {
        var rows = await _db.Applications
            .GroupBy(a => a.Stage)
            .Select(g => new { Stage = g.Key, Count = g.Count() })
            .ToListAsync();

        return rows.ToDictionary(r => r.Stage, r => r.Count);
    }

    public async Task<List<(DateTime AppliedAt, DateTime HiredAt)>> GetHiredDatesAsync()
    {
        var rows = await _db.Applications
            .AsNoTracking()
            .Where(a => a.Stage == ApplicationStage.Hired)
            .Select(a => new { a.AppliedAt, a.StageChangedAt })
            .ToListAsync();

        return rows.Select(r => (r.AppliedAt, r.StageChangedAt)).ToList();
    }

    public async Task AddAsync(JobApplication application)
    {
        await _db.Applications.AddAsync(application);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
