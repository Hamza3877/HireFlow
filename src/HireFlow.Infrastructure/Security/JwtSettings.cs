namespace HireFlow.Infrastructure.Security;

// Values from the "Jwt" section of appsettings. The Key must never be committed to Git.
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "HireFlow";
    public string Audience { get; set; } = "HireFlow.Clients";
    public int ExpiryMinutes { get; set; } = 120;
}
