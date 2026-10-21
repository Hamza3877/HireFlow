using HireFlow.Domain.Enums;

namespace HireFlow.Domain.Entities;

// A job offer for an application. The candidate answers it through a secret, single-use token.
public class Offer
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public JobApplication Application { get; set; } = null!;
    public decimal Salary { get; set; }
    public OfferStatus Status { get; set; } = OfferStatus.Pending;
    public int? ApprovedByUserId { get; set; }
    public string AcceptToken { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
