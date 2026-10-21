namespace HireFlow.Domain.Entities;

// One item on a new hire's onboarding checklist (documents, laptop, accounts...).
public class OnboardingTask
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public JobApplication Application { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; }
}
