using System.Security.Cryptography;
using HireFlow.Application.Dtos;
using HireFlow.Application.Exceptions;
using HireFlow.Application.Interfaces;
using HireFlow.Application.Mappers;
using HireFlow.Application.Notifications;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Services;

// Business rules for offers and the onboarding checklist.
public class OfferService : IOfferService
{
    private static readonly string[] OnboardingTitles =
    {
        "Collect signed documents",
        "Prepare laptop and equipment",
        "Create email and system accounts",
        "Schedule orientation"
    };

    private readonly IOfferRepository _offers;
    private readonly IApplicationRepository _applications;
    private readonly IJobRepository _jobs;
    private readonly IActivityLogService _log;
    private readonly INotificationClient _notifier;

    public OfferService(
        IOfferRepository offers,
        IApplicationRepository applications,
        IJobRepository jobs,
        IActivityLogService log,
        INotificationClient notifier)
    {
        _offers = offers;
        _applications = applications;
        _jobs = jobs;
        _log = log;
        _notifier = notifier;
    }

    public async Task<List<OfferResponse>> GetListAsync(OfferStatus? status)
    {
        var offers = await _offers.GetListAsync(status);
        return offers.Select(o => o.ToResponse()).ToList();
    }

    public async Task<OfferResponse> CreateAsync(CreateOfferRequest request, int userId)
    {
        var application = await _applications.GetByIdAsync(request.ApplicationId)
            ?? throw new NotFoundException("Application not found.");

        if (application.Stage != ApplicationStage.Offer)
        {
            throw new BusinessRuleException("An offer can only be created when the candidate is in the Offer stage.");
        }
        if (await _offers.HasActiveOfferAsync(application.Id))
        {
            throw new BusinessRuleException("This candidate already has an active offer.");
        }
        if (request.Salary <= 0)
        {
            throw new BusinessRuleException("Salary must be greater than zero.");
        }

        var offer = new Offer
        {
            ApplicationId = application.Id,
            Application = application,
            Salary = request.Salary,
            Status = OfferStatus.Pending,
            AcceptToken = CreateSecureToken(),
            CreatedAt = DateTime.UtcNow
        };

        await _offers.AddAsync(offer);
        await _offers.SaveChangesAsync();
        await _log.LogAsync("OfferCreated", $"Offer created for {application.Candidate.FullName}.", userId, application.Id);

        return offer.ToResponse();
    }

    public async Task<OfferResponse> ApproveAsync(int id, int userId, Role userRole)
    {
        if (userRole != Role.HiringManager && userRole != Role.Admin)
        {
            throw new ForbiddenException("Only a hiring manager or admin can approve offers.");
        }

        var offer = await _offers.GetByIdAsync(id) ?? throw new NotFoundException("Offer not found.");

        if (offer.Status != OfferStatus.Pending)
        {
            throw new BusinessRuleException("Only pending offers can be approved.");
        }

        offer.Status = OfferStatus.Approved;
        offer.ApprovedByUserId = userId;
        await _offers.SaveChangesAsync();

        var candidate = offer.Application.Candidate;
        await _log.LogAsync("OfferApproved", $"Offer for {candidate.FullName} approved.", userId, offer.ApplicationId);
        await _notifier.SendAsync(new NotificationMessage(
            "OfferApproved",
            "Your job offer is ready",
            $"Hi {candidate.FullName}, your offer for {offer.Application.Job.Title} is ready. Please open your secure offer link to answer it."));

        return offer.ToResponse();
    }

    public async Task<PublicOfferResponse> GetByTokenAsync(string token)
    {
        var offer = await FindByTokenAsync(token);
        return offer.ToPublicResponse();
    }

    public async Task<PublicOfferResponse> RespondAsync(string token, bool accept)
    {
        var offer = await FindByTokenAsync(token);

        // Single use: once the offer is Accepted or Declined, the link no longer works.
        if (offer.Status != OfferStatus.Approved)
        {
            throw new BusinessRuleException("This offer cannot be answered (it may not be approved yet, or it was already answered).");
        }

        var application = offer.Application;

        if (accept)
        {
            offer.Status = OfferStatus.Accepted;
            application.Stage = ApplicationStage.Hired;
            application.StageChangedAt = DateTime.UtcNow;

            var tasks = OnboardingTitles.Select(title => new OnboardingTask
            {
                ApplicationId = application.Id,
                Title = title,
                IsDone = false
            });
            await _offers.AddTasksAsync(tasks);

            // Close the job when every position is filled (this hire is not saved yet, so add 1).
            var hiredSoFar = await _applications.CountHiredForJobAsync(application.JobId);
            var job = await _jobs.GetByIdAsync(application.JobId);
            if (job != null && hiredSoFar + 1 >= job.Positions)
            {
                job.Status = JobStatus.Closed;
            }
        }
        else
        {
            offer.Status = OfferStatus.Declined;
            application.Stage = ApplicationStage.Rejected;
            application.StageChangedAt = DateTime.UtcNow;
            application.RejectionReason = "Candidate declined the offer.";
        }

        await _offers.SaveChangesAsync();

        var action = accept ? "OfferAccepted" : "OfferDeclined";
        await _log.LogAsync(action, $"{application.Candidate.FullName} {(accept ? "accepted" : "declined")} the offer.", null, application.Id);
        await _notifier.SendAsync(new NotificationMessage(
            action,
            accept ? "Welcome aboard" : "Offer declined",
            accept
                ? $"{application.Candidate.FullName} accepted the offer for {application.Job.Title}. Onboarding checklist created."
                : $"{application.Candidate.FullName} declined the offer for {application.Job.Title}."));

        return offer.ToPublicResponse();
    }

    public async Task<List<OnboardingTaskResponse>> GetTasksAsync(int applicationId)
    {
        var tasks = await _offers.GetTasksAsync(applicationId);
        return tasks.Select(t => t.ToResponse()).ToList();
    }

    public async Task<OnboardingTaskResponse> CompleteTaskAsync(int taskId, int userId)
    {
        var task = await _offers.GetTaskAsync(taskId) ?? throw new NotFoundException("Task not found.");

        if (!task.IsDone)
        {
            task.IsDone = true;
            await _offers.SaveChangesAsync();
            await _log.LogAsync("OnboardingTaskDone", $"Onboarding task '{task.Title}' completed.", userId, task.ApplicationId);
        }

        return task.ToResponse();
    }

    private async Task<Offer> FindByTokenAsync(string token)
    {
        var offer = await _offers.GetByTokenAsync(token);
        return offer ?? throw new NotFoundException("Offer not found.");
    }

    // 32 random bytes turned into URL-safe text. Impossible to guess.
    private static string CreateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
