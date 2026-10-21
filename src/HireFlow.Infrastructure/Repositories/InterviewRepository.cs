using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Infrastructure.Repositories;

// Database access for interviews and scorecards.
public class InterviewRepository : IInterviewRepository
{
    private readonly AppDbContext _db;

    public InterviewRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Interview?> GetByIdAsync(int id)
    {
        return await _db.Interviews
            .Include(i => i.Application).ThenInclude(a => a.Candidate)
            .Include(i => i.Interviewer)
            .Include(i => i.Scorecard)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<Interview>> GetListAsync(int? applicationId, int? interviewerId)
    {
        var query = _db.Interviews
            .AsNoTracking()
            .Include(i => i.Application).ThenInclude(a => a.Candidate)
            .Include(i => i.Interviewer)
            .Include(i => i.Scorecard)
            .AsQueryable();

        if (applicationId.HasValue)
        {
            query = query.Where(i => i.ApplicationId == applicationId.Value);
        }
        if (interviewerId.HasValue)
        {
            query = query.Where(i => i.InterviewerId == interviewerId.Value);
        }

        return await query.OrderByDescending(i => i.StartTime).Take(200).ToListAsync();
    }

    // Two time ranges overlap when each one starts before the other ends.
    public async Task<bool> HasOverlapAsync(int interviewerId, DateTime start, DateTime end, int? ignoreInterviewId)
    {
        return await _db.Interviews.AnyAsync(i =>
            i.InterviewerId == interviewerId &&
            i.Status == InterviewStatus.Scheduled &&
            (ignoreInterviewId == null || i.Id != ignoreInterviewId) &&
            i.StartTime < end &&
            start < i.EndTime);
    }

    public async Task AddAsync(Interview interview)
    {
        await _db.Interviews.AddAsync(interview);
    }

    public async Task AddScorecardAsync(Scorecard scorecard)
    {
        await _db.Scorecards.AddAsync(scorecard);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
