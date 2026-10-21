using HireFlow.Api.Extensions;
using HireFlow.Application.Common;
using HireFlow.Application.Dtos;
using HireFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.Api.Controllers;

// Staff endpoints for the candidate pipeline (Kanban board and candidate profile).
[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applications;

    public ApplicationsController(IApplicationService applications)
    {
        _applications = applications;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ApplicationResponse>>> GetList([FromQuery] ApplicationListQuery query)
    {
        return Ok(await _applications.GetPagedAsync(query));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApplicationResponse>> GetById(int id)
    {
        return Ok(await _applications.GetByIdAsync(id));
    }

    [HttpPost("{id:int}/move")]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<ActionResult<ApplicationResponse>> MoveStage(int id, MoveStageRequest request)
    {
        return Ok(await _applications.MoveStageAsync(id, request, User.GetUserId(), User.GetRole()));
    }

    [HttpGet("{id:int}/timeline")]
    public async Task<ActionResult<List<ActivityLogResponse>>> GetTimeline(int id)
    {
        return Ok(await _applications.GetTimelineAsync(id));
    }

    [HttpGet("{id:int}/cv")]
    public async Task<IActionResult> DownloadCv(int id)
    {
        var (file, fileName) = await _applications.OpenCvAsync(id);
        var contentType = fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        return File(file, contentType, fileName);
    }
}
