using HireFlow.Application.Dtos;

namespace HireFlow.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<List<UserResponse>> GetStaffAsync();
}
