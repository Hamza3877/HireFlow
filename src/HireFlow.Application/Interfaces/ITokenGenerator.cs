using HireFlow.Domain.Entities;

namespace HireFlow.Application.Interfaces;

public class TokenResult
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

// Creates the JWT a user receives after logging in.
public interface ITokenGenerator
{
    TokenResult Generate(User user);
}
