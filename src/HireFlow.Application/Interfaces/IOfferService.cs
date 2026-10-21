using HireFlow.Application.Dtos;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Interfaces;

public interface IOfferService
{
    Task<List<OfferResponse>> GetListAsync(OfferStatus? status);
    Task<OfferResponse> CreateAsync(CreateOfferRequest request, int userId);
    Task<OfferResponse> ApproveAsync(int id, int userId, Role userRole);
    Task<PublicOfferResponse> GetByTokenAsync(string token);
    Task<PublicOfferResponse> RespondAsync(string token, bool accept);
    Task<List<OnboardingTaskResponse>> GetTasksAsync(int applicationId);
    Task<OnboardingTaskResponse> CompleteTaskAsync(int taskId, int userId);
}
