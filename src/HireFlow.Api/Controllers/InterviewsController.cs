using HireFlow.Api.Extensions;
using HireFlow.Application.Dtos;
using HireFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.Api.Controllers;

// Staff endpoints for interviews and scorecards.
[ApiController]
[Route("api/interviews")]
[Authorize]
public class InterviewsController : ControllerBase
{
    private readonly IInterviewService _interviews;

    public InterviewsController(IInterviewService interviews)
    {
        _interviews = interviews;
    }

    [HttpGet]
    public async Task<ActionResult<List<InterviewResponse>>> GetList([FromQuery] int? applicationId, [FromQuery] int? interviewerId)
    {
        return Ok(await _interviews.GetListAsync(applicationId, interviewerId));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<ActionResult<InterviewResponse>> Schedule(ScheduleInterviewRequest request)
    {
        return Ok(await _interviews.ScheduleAsync(request, User.GetUserId()));
    }

    [HttpPost("{id:int}/reschedule")]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<ActionResult<InterviewResponse>> Reschedule(int id, RescheduleInterviewRequest request)
    {
        return Ok(await _interviews.RescheduleAsync(id, request, User.GetUserId()));
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<ActionResult<InterviewResponse>> Cancel(int id)
    {
        return Ok(await _interviews.CancelAsync(id, User.GetUserId()));
    }

    // Any staff member can call this, but the service only accepts the assigned interviewer.
    [HttpPost("{id:int}/scorecard")]
    public async Task<ActionResult<InterviewResponse>> SubmitScorecard(int id, ScorecardRequest request)
    {
        return Ok(await _interviews.SubmitScorecardAsync(id, request, User.GetUserId()));
    }
}
