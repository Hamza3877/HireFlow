using System.Security.Claims;
using HireFlow.Domain.Enums;

namespace HireFlow.Api.Extensions;

// Small helpers so controllers can read "who is calling" from the JWT in one line.
public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("uid")?.Value;
        return int.TryParse(value, out var id) ? id : 0;
    }

    public static Role GetRole(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("role")?.Value;
        return Enum.TryParse<Role>(value, out var role) ? role : Role.Recruiter;
    }
}
