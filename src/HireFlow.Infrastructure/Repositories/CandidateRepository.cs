using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Infrastructure.Repositories;

// Database access for candidates.
public class CandidateRepository : ICandidateRepository
{
    private readonly AppDbContext _db;

    public CandidateRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Candidate?> GetByEmailAsync(string email)
    {
        return await _db.Candidates.FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task AddAsync(Candidate candidate)
    {
        await _db.Candidates.AddAsync(candidate);
    }
}
