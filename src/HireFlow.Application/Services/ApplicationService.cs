using HireFlow.Application.Common;
using HireFlow.Application.Dtos;
using HireFlow.Application.Exceptions;
using HireFlow.Application.Interfaces;
using HireFlow.Application.Mappers;
using HireFlow.Application.Notifications;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Services;

// Business rules for job applications: applying, the match score and moving through stages.
public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _applications;
    private readonly ICandidateRepository _candidates;
    private readonly IJobRepository _jobs;
    private readonly ICvStorage _cvStorage;
    private readonly ICvTextExtractor _cvExtractor;
    private readonly IMatchScoreCalculator _scoreCalculator;
    private readonly IActivityLogService _log;
    private readonly INotificationClient _notifier;

    public ApplicationService(
        IApplicationRepository applications,
        ICandidateRepository candidates,
        IJobRepository jobs,
        ICvStorage cvStorage,
        ICvTextExtractor cvExtractor,
        IMatchScoreCalculator scoreCalculator,
        IActivityLogService log,
        INotificationClient notifier)
    {
        _applications = applications;
        _candidates = candidates;
        _jobs = jobs;
        _cvStorage = cvStorage;
        _cvExtractor = cvExtractor;
        _scoreCalculator = scoreCalculator;
        _log = log;
        _notifier = notifier;
    }

    public async Task<ApplicationResponse> ApplyAsync(int jobId, ApplyRequest request, CvUpload cv)
    {
        var job = await _jobs.GetByIdAsync(jobId) ?? throw new NotFoundException("Job not found.");

        if (job.Status != JobStatus.Open)
        {
            throw new BusinessRuleException("This job is not open for applications.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _applications.ExistsAsync(jobId, email))
        {
            throw new BusinessRuleException("You have already applied for this job.");
        }

        // Save the CV (this also checks type and size), then read its text for the match score.
        var storedFileName = await _cvStorage.SaveAsync(cv);
        string cvText;
        using (var stream = _cvStorage.Open(storedFileName))
        {
            cvText = _cvExtractor.Extract(stream, Path.GetExtension(storedFileName));
        }

        var candidate = await _candidates.GetByEmailAsync(email);
        if (candidate == null)
        {
            candidate = new Candidate
            {
                FullName = request.FullName.Trim(),
                Email = email,
                Phone = request.Phone.Trim()
            };
            await _candidates.AddAsync(candidate);
        }

        var now = DateTime.UtcNow;
        var application = new JobApplication
        {
            Job = job,
            Candidate = candidate,
            Stage = ApplicationStage.Applied,
            MatchScore = _scoreCalculator.Calculate(job.RequiredSkills, cvText),
            CvFilePath = storedFileName,
            CvText = cvText,
            ApplicationCode = CreateApplicationCode(),
            AppliedAt = now,
            StageChangedAt = now
        };

        await _applications.AddAsync(application);
        await _applications.SaveChangesAsync();

        await _log.LogAsync("Applied", $"{candidate.FullName} applied for '{job.Title}'.", null, application.Id);
        await _notifier.SendAsync(new NotificationMessage(
            "ApplicationReceived",
            "We received your application",
            $"Hi {candidate.FullName}, we received your application for {job.Title}. Your code is {application.ApplicationCode}."));

        return application.ToResponse();
    }

    public async Task<PagedResult<ApplicationResponse>> GetPagedAsync(ApplicationListQuery query)
    {
        var page = Paging.SafePage(query.Page);
        var pageSize = Paging.SafePageSize(query.PageSize);

        var (items, total) = await _applications.GetPagedAsync(query.Search, query.Stage, query.JobId, page, pageSize);

        return new PagedResult<ApplicationResponse>
        {
            Items = items.Select(a => a.ToResponse()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<ApplicationResponse> GetByIdAsync(int id)
    {
        var application = await FindApplicationAsync(id);
        return application.ToResponse();
    }

    public async Task<ApplicationResponse> MoveStageAsync(int id, MoveStageRequest request, int userId, Role userRole)
    {
        if (userRole != Role.Recruiter && userRole != Role.Admin)
        {
            throw new ForbiddenException("Only a recruiter or admin can move candidates between stages.");
        }

        var application = await FindApplicationAsync(id);

        if (application.Stage == ApplicationStage.Hired)
        {
            throw new BusinessRuleException("A hired candidate cannot be moved.");
        }
        if (request.Stage == ApplicationStage.Hired)
        {
            throw new BusinessRuleException("A candidate becomes Hired when they accept an offer.");
        }
        if (request.Stage == application.Stage)
        {
            throw new BusinessRuleException("The candidate is already in this stage.");
        }
        if (request.Stage == ApplicationStage.Rejected && string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new BusinessRuleException("A reason is required to reject a candidate.");
        }
        if (request.Stage == ApplicationStage.Offer && !await _applications.HasSubmittedScorecardAsync(id))
        {
            throw new BusinessRuleException("A candidate needs at least one submitted scorecard before moving to Offer.");
        }

        var oldStage = application.Stage;
        application.Stage = request.Stage;
        application.StageChangedAt = DateTime.UtcNow;
        application.RejectionReason = request.Stage == ApplicationStage.Rejected ? request.Reason!.Trim() : null;

        await _applications.SaveChangesAsync();

        await _log.LogAsync(
            "StageChanged",
            $"{application.Candidate.FullName} moved from {oldStage} to {request.Stage}.",
            userId,
            application.Id);
        await _notifier.SendAsync(new NotificationMessage(
            "StageChanged",
            "Your application status changed",
            $"Hi {application.Candidate.FullName}, your application for {application.Job.Title} is now: {request.Stage}."));

        return application.ToResponse();
    }

    public async Task<ApplicationStatusResponse> GetStatusAsync(string email, string code)
    {
        var application = await _applications.GetByEmailAndCodeAsync(email.Trim().ToLowerInvariant(), code.Trim().ToUpperInvariant());
        if (application == null)
        {
            throw new NotFoundException("No application found for this email and code.");
        }

        return new ApplicationStatusResponse
        {
            JobTitle = application.Job.Title,
            Stage = application.Stage,
            AppliedAt = application.AppliedAt
        };
    }

    public async Task<List<ActivityLogResponse>> GetTimelineAsync(int id)
    {
        await FindApplicationAsync(id);
        return await _log.GetForApplicationAsync(id);
    }

    public async Task<(Stream File, string FileName)> OpenCvAsync(int id)
    {
        var application = await FindApplicationAsync(id);
        var extension = Path.GetExtension(application.CvFilePath);
        var downloadName = $"{application.Candidate.FullName}-CV{extension}";
        return (_cvStorage.Open(application.CvFilePath), downloadName);
    }

    private async Task<JobApplication> FindApplicationAsync(int id)
    {
        var application = await _applications.GetByIdAsync(id);
        return application ?? throw new NotFoundException("Application not found.");
    }

    // 8 characters, for example "7F3K9QZ2". Easy to read out loud, hard to guess.
    private static string CreateApplicationCode()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }
}
