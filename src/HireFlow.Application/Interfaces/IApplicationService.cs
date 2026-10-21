using HireFlow.Application.Common;
using HireFlow.Application.Dtos;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Interfaces;

public interface IApplicationService
{
    Task<ApplicationResponse> ApplyAsync(int jobId, ApplyRequest request, CvUpload cv);
    Task<PagedResult<ApplicationResponse>> GetPagedAsync(ApplicationListQuery query);
    Task<ApplicationResponse> GetByIdAsync(int id);
    Task<ApplicationResponse> MoveStageAsync(int id, MoveStageRequest request, int userId, Role userRole);
    Task<ApplicationStatusResponse> GetStatusAsync(string email, string code);
    Task<List<ActivityLogResponse>> GetTimelineAsync(int id);
    Task<(Stream File, string FileName)> OpenCvAsync(int id);
}
