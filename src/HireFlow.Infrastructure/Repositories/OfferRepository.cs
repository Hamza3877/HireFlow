using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Infrastructure.Repositories;

// Database access for offers and onboarding tasks.
public class OfferRepository : IOfferRepository
{
    private readonly AppDbContext _db;

    public OfferRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Offer?> GetByIdAsync(int id)
    {
        return await OffersWithDetails().FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Offer?> GetByTokenAsync(string token)
    {
        return await OffersWithDetails().FirstOrDefaultAsync(o => o.AcceptToken == token);
    }

    public async Task<bool> HasActiveOfferAsync(int applicationId)
    {
        return await _db.Offers.AnyAsync(o =>
            o.ApplicationId == applicationId &&
            (o.Status == OfferStatus.Pending || o.Status == OfferStatus.Approved));
    }

    public async Task<List<Offer>> GetListAsync(OfferStatus? status)
    {
        var query = OffersWithDetails().AsNoTracking();
        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }
        return await query.OrderByDescending(o => o.CreatedAt).Take(200).ToListAsync();
    }

    public async Task<int> CountByStatusAsync(OfferStatus status)
    {
        return await _db.Offers.CountAsync(o => o.Status == status);
    }

    public async Task AddAsync(Offer offer)
    {
        await _db.Offers.AddAsync(offer);
    }

    public async Task<List<OnboardingTask>> GetTasksAsync(int applicationId)
    {
        return await _db.OnboardingTasks
            .AsNoTracking()
            .Where(t => t.ApplicationId == applicationId)
            .OrderBy(t => t.Id)
            .ToListAsync();
    }

    public async Task<OnboardingTask?> GetTaskAsync(int taskId)
    {
        return await _db.OnboardingTasks.FirstOrDefaultAsync(t => t.Id == taskId);
    }

    public async Task AddTasksAsync(IEnumerable<OnboardingTask> tasks)
    {
        await _db.OnboardingTasks.AddRangeAsync(tasks);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }

    private IQueryable<Offer> OffersWithDetails()
    {
        return _db.Offers
            .Include(o => o.Application).ThenInclude(a => a.Candidate)
            .Include(o => o.Application).ThenInclude(a => a.Job);
    }
}
