using HireFlow.Api.Extensions;
using HireFlow.Application.Common;
using HireFlow.Application.Dtos;
using HireFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.Api.Controllers;

// Staff endpoints for jobs. Thin on purpose: every rule lives in JobService.
[ApiController]
[Route("api/jobs")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobs;

    public JobsController(IJobService jobs)
    {
        _jobs = jobs;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<JobResponse>>> GetList([FromQuery] JobListQuery query)
    {
        return Ok(await _jobs.GetPagedAsync(query));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<JobResponse>> GetById(int id)
    {
        return Ok(await _jobs.GetByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<ActionResult<JobResponse>> Create(JobRequest request)
    {
        var job = await _jobs.CreateAsync(request, User.GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = job.Id }, job);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<ActionResult<JobResponse>> Update(int id, JobRequest request)
    {
        return Ok(await _jobs.UpdateAsync(id, request, User.GetUserId()));
    }

    [HttpPost("{id:int}/submit")]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<ActionResult<JobResponse>> Submit(int id)
    {
        return Ok(await _jobs.SubmitForApprovalAsync(id, User.GetUserId()));
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Admin,HiringManager")]
    public async Task<ActionResult<JobResponse>> Approve(int id)
    {
        return Ok(await _jobs.ApproveAsync(id, User.GetUserId(), User.GetRole()));
    }

    [HttpPost("{id:int}/close")]
    public async Task<ActionResult<JobResponse>> Close(int id)
    {
        return Ok(await _jobs.CloseAsync(id, User.GetUserId()));
    }
}
