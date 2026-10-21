using HireFlow.Domain.Entities;

namespace HireFlow.Application.Interfaces;

public interface ICandidateRepository
{
    Task<Candidate?> GetByEmailAsync(string email);
    Task AddAsync(Candidate candidate);
}
