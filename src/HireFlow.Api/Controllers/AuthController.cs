using HireFlow.Application.Dtos;
using HireFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HireFlow.Api.Controllers;

// Login and the staff list. The controller only passes data to the service.
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        return Ok(await _auth.LoginAsync(request));
    }

    // Used by screens that need a list of staff, for example to pick an interviewer.
    [HttpGet("staff")]
    [Authorize]
    public async Task<ActionResult<List<UserResponse>>> GetStaff()
    {
        return Ok(await _auth.GetStaffAsync());
    }
}
