using HireFlow.Application.Dtos;
using HireFlow.Application.Exceptions;
using HireFlow.Application.Interfaces;
using HireFlow.Application.Services;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace HireFlow.Tests;

// Tests for login.
public class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ITokenGenerator _tokens = Substitute.For<ITokenGenerator>();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _tokens.Generate(Arg.Any<User>()).Returns(new TokenResult { Token = "fake-token", ExpiresAt = DateTime.UtcNow.AddHours(1) });
        _service = new AuthService(_users, _tokens);
    }

    private static User UserWithPassword(string password)
    {
        var user = new User { Id = 1, FullName = "Admin", Email = "admin@hireflow.test", Role = Role.Admin };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        return user;
    }

    [Fact]
    public async Task Login_WithWrongPassword_Throws()
    {
        _users.GetByEmailAsync("admin@hireflow.test").Returns(UserWithPassword("Correct@123"));

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest { Email = "admin@hireflow.test", Password = "wrong" }));
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Throws()
    {
        _users.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest { Email = "nobody@hireflow.test", Password = "x" }));
    }

    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsToken()
    {
        _users.GetByEmailAsync("admin@hireflow.test").Returns(UserWithPassword("Correct@123"));

        var result = await _service.LoginAsync(new LoginRequest { Email = "admin@hireflow.test", Password = "Correct@123" });

        Assert.Equal("fake-token", result.Token);
        Assert.Equal(Role.Admin, result.Role);
    }
}
