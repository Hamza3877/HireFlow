using HireFlow.Application.Dtos;
using HireFlow.Application.Exceptions;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace HireFlow.Application.Services;

// Checks the email and password and hands out a JWT token.
public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ITokenGenerator _tokens;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthService(IUserRepository users, ITokenGenerator tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());

        // Same message for "unknown email" and "wrong password" so attackers learn nothing.
        if (user == null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var token = _tokens.Generate(user);
        return new LoginResponse
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            UserId = user.Id,
            FullName = user.FullName,
            Role = user.Role
        };
    }

    public async Task<List<UserResponse>> GetStaffAsync()
    {
        var users = await _users.GetAllAsync();
        return users
            .Select(u => new UserResponse { Id = u.Id, FullName = u.FullName, Role = u.Role })
            .ToList();
    }
}
