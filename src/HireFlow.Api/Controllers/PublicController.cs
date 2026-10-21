using System.ComponentModel.DataAnnotations;
using HireFlow.Application.Common;
using HireFlow.Application.Dtos;
using HireFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HireFlow.Api.Controllers;

// Public endpoints used by the careers website. No login needed, so only safe data is returned.
[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class PublicController : ControllerBase
{
    private readonly IJobService _jobs;
    private readonly IApplicationService _applications;
    private readonly IOfferService _offers;

    public PublicController(IJobService jobs, IApplicationService applications, IOfferService offers)
    {
        _jobs = jobs;
        _applications = applications;
        _offers = offers;
    }

    [HttpGet("jobs")]
    public async Task<ActionResult<PagedResult<JobResponse>>> GetOpenJobs([FromQuery] JobListQuery query)
    {
        return Ok(await _jobs.GetOpenJobsAsync(query));
    }

    [HttpGet("jobs/{id:int}")]
    public async Task<ActionResult<JobResponse>> GetOpenJob(int id)
    {
        return Ok(await _jobs.GetOpenJobByIdAsync(id));
    }

    [HttpPost("jobs/{id:int}/apply")]
    [EnableRateLimiting("apply")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApplicationStatusAfterApply>> Apply(int id, [FromForm] ApplyForm form)
    {
        var request = new ApplyRequest
        {
            FullName = form.FullName,
            Email = form.Email,
            Phone = form.Phone
        };

        await using var stream = form.Cv.OpenReadStream();
        var cv = new CvUpload
        {
            Content = stream,
            FileName = form.Cv.FileName,
            ContentType = form.Cv.ContentType,
            Length = form.Cv.Length
        };

        var application = await _applications.ApplyAsync(id, request, cv);

        // The candidate only needs to know the code and the job. Nothing internal.
        return Ok(new ApplicationStatusAfterApply
        {
            ApplicationCode = application.ApplicationCode,
            JobTitle = application.JobTitle
        });
    }

    [HttpGet("applications/status")]
    public async Task<ActionResult<ApplicationStatusResponse>> GetStatus([FromQuery, Required] string email, [FromQuery, Required] string code)
    {
        return Ok(await _applications.GetStatusAsync(email, code));
    }

    [HttpGet("offers/{token}")]
    public async Task<ActionResult<PublicOfferResponse>> GetOffer(string token)
    {
        return Ok(await _offers.GetByTokenAsync(token));
    }

    [HttpPost("offers/{token}/respond")]
    public async Task<ActionResult<PublicOfferResponse>> RespondToOffer(string token, OfferDecisionRequest request)
    {
        return Ok(await _offers.RespondAsync(token, request.Accept));
    }
}

// The form fields of the apply page (multipart/form-data, because it includes a file).
public class ApplyForm
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public IFormFile Cv { get; set; } = null!;
}

public class ApplicationStatusAfterApply
{
    public string ApplicationCode { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
}
