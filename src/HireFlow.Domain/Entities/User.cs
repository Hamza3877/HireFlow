using HireFlow.Domain.Enums;

namespace HireFlow.Domain.Entities;

// A staff member who can log in (Admin, Recruiter or HiringManager).
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; }
}
