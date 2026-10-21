using HireFlow.Application.Services;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Infrastructure.Data;

// Fills an empty database with realistic demo data: 3 users, 6 jobs, 40 candidates.
public static class DatabaseSeeder
{
    public const string DemoPassword = "Demo@12345";

    private static readonly string[] FirstNames =
        { "Ali", "Ayesha", "Hamza", "Fatima", "Usman", "Sana", "Bilal", "Hira", "Zain", "Maryam" };

    private static readonly string[] LastNames =
        { "Khan", "Ahmed", "Malik", "Sheikh", "Butt", "Raza", "Qureshi", "Hussain", "Iqbal", "Siddiqui" };

    // Spread of stages so every Kanban column has cards.
    private static readonly ApplicationStage[] StagePattern =
    {
        ApplicationStage.Applied, ApplicationStage.Applied, ApplicationStage.Applied, ApplicationStage.Screening,
        ApplicationStage.Screening, ApplicationStage.Interview, ApplicationStage.Interview, ApplicationStage.Offer,
        ApplicationStage.Hired, ApplicationStage.Rejected
    };

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var users = CreateUsers();
        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        var recruiter = users.First(u => u.Role == Role.Recruiter);
        var jobs = CreateJobs(users.First(u => u.Role == Role.Admin).Id);
        db.Jobs.AddRange(jobs);
        await db.SaveChangesAsync();

        var openJobs = jobs.Where(j => j.Status == JobStatus.Open).ToList();
        var calculator = new MatchScoreCalculator();

        for (var i = 0; i < 40; i++)
        {
            var job = openJobs[i % openJobs.Count];
            var stage = StagePattern[i % StagePattern.Length];
            var appliedAt = DateTime.UtcNow.AddDays(-(60 - i));

            var candidate = new Candidate
            {
                FullName = $"{FirstNames[i % 10]} {LastNames[(i * 3) % 10]}",
                Email = $"{FirstNames[i % 10]}.{LastNames[(i * 3) % 10]}{i + 1}@example.com".ToLowerInvariant(),
                Phone = $"0300{1000000 + i * 7919:D7}"
            };

            var cvText = BuildCvText(job, i);
            var application = new JobApplication
            {
                Job = job,
                Candidate = candidate,
                Stage = stage,
                CvFilePath = "demo-cv.pdf",
                CvText = cvText,
                MatchScore = calculator.Calculate(job.RequiredSkills, cvText),
                ApplicationCode = $"HF{1000 + i}",
                AppliedAt = appliedAt,
                StageChangedAt = stage == ApplicationStage.Applied ? appliedAt : appliedAt.AddDays(2 + i % 5)
            };

            if (stage == ApplicationStage.Hired)
            {
                application.StageChangedAt = appliedAt.AddDays(12 + i % 8);
            }
            if (stage == ApplicationStage.Rejected)
            {
                application.RejectionReason = "Skills do not match the role.";
            }

            db.Applications.Add(application);

            // Candidates at Offer or Hired stage must have had an interview with a scorecard.
            if (stage == ApplicationStage.Offer || stage == ApplicationStage.Hired)
            {
                db.Interviews.Add(new Interview
                {
                    Application = application,
                    InterviewerId = recruiter.Id,
                    StartTime = appliedAt.AddDays(3),
                    EndTime = appliedAt.AddDays(3).AddHours(1),
                    Status = InterviewStatus.Completed,
                    Scorecard = new Scorecard
                    {
                        Rating = 4,
                        Comments = "Strong technical answers and clear communication.",
                        Recommendation = "Hire"
                    }
                });
            }
        }
        await db.SaveChangesAsync();

        db.ActivityLogs.Add(new ActivityLog
        {
            Action = "SystemSeeded",
            Details = "Demo data created.",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static List<User> CreateUsers()
    {
        var hasher = new PasswordHasher<User>();
        var users = new List<User>
        {
            new() { FullName = "Admin User", Email = "admin@hireflow.test", Role = Role.Admin },
            new() { FullName = "Rida Recruiter", Email = "recruiter@hireflow.test", Role = Role.Recruiter },
            new() { FullName = "Hassan Manager", Email = "manager@hireflow.test", Role = Role.HiringManager }
        };

        foreach (var user in users)
        {
            user.PasswordHash = hasher.HashPassword(user, DemoPassword);
        }
        return users;
    }

    private static List<Job> CreateJobs(int createdByUserId)
    {
        Job NewJob(string title, string department, string location, string skills, JobStatus status) => new()
        {
            Title = title,
            Department = department,
            Location = location,
            Description = $"We are looking for a {title} to join our {department} team in {location}. " +
                          "You will work with a friendly team on real products used by real customers.",
            RequiredSkills = skills,
            Positions = 5,
            Status = status,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow.AddDays(-70)
        };

        return new List<Job>
        {
            NewJob(".NET Developer", "Engineering", "Karachi", "C#, ASP.NET Core, SQL Server, Entity Framework, REST API, Git", JobStatus.Open),
            NewJob("Angular Developer", "Engineering", "Lahore", "Angular, TypeScript, RxJS, HTML, CSS, REST API", JobStatus.Open),
            NewJob("QA Engineer", "Quality", "Islamabad", "Selenium, Test Cases, SQL, Postman, Agile, Jira", JobStatus.Open),
            NewJob("HR Executive", "People", "Karachi", "Recruitment, Communication, Excel, Onboarding", JobStatus.PendingApproval),
            NewJob("DevOps Engineer", "Infrastructure", "Remote", "Docker, CI/CD, Azure, Linux, Git", JobStatus.Draft),
            NewJob("Business Analyst", "Product", "Lahore", "Requirements, SQL, Documentation, Agile", JobStatus.Closed)
        };
    }

    // Different candidates mention different numbers of the job's skills, so match scores vary.
    private static string BuildCvText(Job job, int index)
    {
        var skills = job.RequiredSkills.Split(',', StringSplitOptions.TrimEntries).ToList();
        var howMany = (index * 7) % (skills.Count + 1);
        var mentioned = string.Join(", ", skills.Take(howMany));
        return $"Experienced professional. Skills: {mentioned}. Worked in agile teams and delivered projects on time.";
    }
}
