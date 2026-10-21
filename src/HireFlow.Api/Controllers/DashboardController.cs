using HireFlow.Application.Dtos;
using HireFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.Api.Controllers;

// Numbers for the dashboard and the admin activity log.
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;
    private readonly IActivityLogService _log;

    public DashboardController(IDashboardService dashboard, IActivityLogService log)
    {
        _dashboard = dashboard;
        _log = log;
    }

    [HttpGet("api/dashboard")]
    public async Task<ActionResult<DashboardResponse>> Get()
    {
        return Ok(await _dashboard.GetAsync());
    }

    [HttpGet("api/activity-log")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<ActivityLogResponse>>> GetActivityLog([FromQuery] int count = 50)
    {
        return Ok(await _log.GetRecentAsync(count));
    }
}
