using HireFlow.Domain.Enums;

namespace HireFlow.Application.Dtos;

public class StageCount
{
    public ApplicationStage Stage { get; set; }
    public int Count { get; set; }
}

public class DashboardResponse
{
    public int OpenJobs { get; set; }
    public int JobsWaitingApproval { get; set; }
    public int TotalCandidates { get; set; }
    public int OffersWaitingApproval { get; set; }
    public double AverageDaysToHire { get; set; }
    public List<StageCount> CandidatesPerStage { get; set; } = new();
}
