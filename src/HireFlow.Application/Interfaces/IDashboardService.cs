using HireFlow.Application.Dtos;

namespace HireFlow.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponse> GetAsync();
}
