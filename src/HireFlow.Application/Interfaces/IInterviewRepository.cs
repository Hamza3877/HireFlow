using HireFlow.Domain.Entities;

namespace HireFlow.Application.Interfaces;

public interface IInterviewRepository
{
    Task<Interview?> GetByIdAsync(int id);
    Task<List<Interview>> GetListAsync(int? applicationId, int? interviewerId);
    Task<bool> HasOverlapAsync(int interviewerId, DateTime start, DateTime end, int? ignoreInterviewId);
    Task AddAsync(Interview interview);
    Task AddScorecardAsync(Scorecard scorecard);
    Task SaveChangesAsync();
}
