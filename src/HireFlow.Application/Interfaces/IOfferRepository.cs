using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Interfaces;

public interface IOfferRepository
{
    Task<Offer?> GetByIdAsync(int id);
    Task<Offer?> GetByTokenAsync(string token);
    Task<bool> HasActiveOfferAsync(int applicationId);
    Task<List<Offer>> GetListAsync(OfferStatus? status);
    Task<int> CountByStatusAsync(OfferStatus status);
    Task AddAsync(Offer offer);
    Task<List<OnboardingTask>> GetTasksAsync(int applicationId);
    Task<OnboardingTask?> GetTaskAsync(int taskId);
    Task AddTasksAsync(IEnumerable<OnboardingTask> tasks);
    Task SaveChangesAsync();
}
