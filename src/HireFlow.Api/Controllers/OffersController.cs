using HireFlow.Api.Extensions;
using HireFlow.Application.Dtos;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.Api.Controllers;

// Staff endpoints for offers and the onboarding checklist.
[ApiController]
[Authorize]
public class OffersController : ControllerBase
{
    private readonly IOfferService _offers;

    public OffersController(IOfferService offers)
    {
        _offers = offers;
    }

    [HttpGet("api/offers")]
    public async Task<ActionResult<List<OfferResponse>>> GetList([FromQuery] OfferStatus? status)
    {
        return Ok(await _offers.GetListAsync(status));
    }

    [HttpPost("api/offers")]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<ActionResult<OfferResponse>> Create(CreateOfferRequest request)
    {
        return Ok(await _offers.CreateAsync(request, User.GetUserId()));
    }

    [HttpPost("api/offers/{id:int}/approve")]
    [Authorize(Roles = "Admin,HiringManager")]
    public async Task<ActionResult<OfferResponse>> Approve(int id)
    {
        return Ok(await _offers.ApproveAsync(id, User.GetUserId(), User.GetRole()));
    }

    [HttpGet("api/onboarding/{applicationId:int}")]
    public async Task<ActionResult<List<OnboardingTaskResponse>>> GetTasks(int applicationId)
    {
        return Ok(await _offers.GetTasksAsync(applicationId));
    }

    [HttpPost("api/onboarding/tasks/{taskId:int}/complete")]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<ActionResult<OnboardingTaskResponse>> CompleteTask(int taskId)
    {
        return Ok(await _offers.CompleteTaskAsync(taskId, User.GetUserId()));
    }
}
