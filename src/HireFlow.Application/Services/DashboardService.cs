using HireFlow.Application.Dtos;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Enums;

namespace HireFlow.Application.Services;

// Collects the numbers shown on the recruiter dashboard.
public class DashboardService : IDashboardService
{
    private readonly IJobRepository _jobs;
    private readonly IApplicationRepository _applications;
    private readonly IOfferRepository _offers;

    public DashboardService(IJobRepository jobs, IApplicationRepository applications, IOfferRepository offers)
    {
        _jobs = jobs;
        _applications = applications;
        _offers = offers;
    }

    public async Task<DashboardResponse> GetAsync()
    {
        var perStage = await _applications.CountByStageAsync();
        var hiredDates = await _applications.GetHiredDatesAsync();

        // Show every stage, even when it has 0 candidates, so the chart is always complete.
        var stageCounts = Enum.GetValues<ApplicationStage>()
            .Select(stage => new StageCount
            {
                Stage = stage,
                Count = perStage.TryGetValue(stage, out var count) ? count : 0
            })
            .ToList();

        var averageDays = hiredDates.Count == 0
            ? 0
            : Math.Round(hiredDates.Average(h => (h.HiredAt - h.AppliedAt).TotalDays), 1);

        return new DashboardResponse
        {
            OpenJobs = await _jobs.CountByStatusAsync(JobStatus.Open),
            JobsWaitingApproval = await _jobs.CountByStatusAsync(JobStatus.PendingApproval),
            TotalCandidates = await _applications.CountAllAsync(),
            OffersWaitingApproval = await _offers.CountByStatusAsync(OfferStatus.Pending),
            AverageDaysToHire = averageDays,
            CandidatesPerStage = stageCounts
        };
    }
}
