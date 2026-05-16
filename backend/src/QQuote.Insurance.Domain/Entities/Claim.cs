using QQuote.Insurance.Domain.Exceptions;

namespace QQuote.Insurance.Domain.Entities;

public class Claim
{
    public Guid        Id           { get; private set; }
    public Guid        PolicyId     { get; private set; }
    public DateTime    IncidentDate { get; private set; }
    public string      Description  { get; private set; } = string.Empty;
    public ClaimStatus Status       { get; private set; }
    public DateTime    CreatedAt    { get; private set; }

    private Claim() { }

    public static Claim File(Guid policyId, DateTime incidentDate, string description)
    {
        if (policyId == Guid.Empty)
            throw new DomainException("PolicyId is required.");
        if (incidentDate > DateTime.UtcNow)
            throw new DomainException("Incident date cannot be in the future.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");

        return new Claim
        {
            Id           = Guid.NewGuid(),
            PolicyId     = policyId,
            IncidentDate = incidentDate,
            Description  = description.Trim(),
            Status       = ClaimStatus.Reported,
            CreatedAt    = DateTime.UtcNow
        };
    }

    public void Resolve()     => Status = ClaimStatus.Resolved;
    public void StartReview() => Status = ClaimStatus.UnderReview;
}

public enum ClaimStatus { Reported, UnderReview, Resolved }
