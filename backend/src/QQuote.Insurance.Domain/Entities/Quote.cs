using QQuote.Insurance.Domain.Exceptions;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Domain.Entities;

public class Quote
{
    public Guid         Id              { get; private set; }
    public Guid         CustomerId      { get; private set; }
    public Vehicle      Vehicle         { get; private set; } = null!;
    public CoverageType CoverageType    { get; private set; } = null!;
    public Money        MonthlyPremium  { get; private set; } = null!;
    public RiskLevel    RiskLevel       { get; private set; } = null!;
    public string       RiskExplanation { get; private set; } = string.Empty;
    public QuoteStatus  Status          { get; private set; }
    public DateTime     CreatedAt       { get; private set; }
    public DateTime     ExpiresAt       { get; private set; }

    private Quote() { }

    public static Quote Create(
        Guid         customerId,
        Vehicle      vehicle,
        CoverageType coverageType,
        Money        monthlyPremium,
        RiskLevel    riskLevel,
        string       riskExplanation)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("CustomerId is required.");
        if (string.IsNullOrWhiteSpace(riskExplanation))
            throw new DomainException("Risk explanation is required.");

        return new Quote
        {
            Id              = Guid.NewGuid(),
            CustomerId      = customerId,
            Vehicle         = vehicle,
            CoverageType    = coverageType,
            MonthlyPremium  = monthlyPremium,
            RiskLevel       = riskLevel,
            RiskExplanation = riskExplanation,
            Status          = QuoteStatus.Active,
            CreatedAt       = DateTime.UtcNow,
            ExpiresAt       = DateTime.UtcNow.AddDays(30)
        };
    }

    public void ConvertToPolicy()
    {
        if (Status != QuoteStatus.Active)
            throw new DomainException("Only active quotes can be converted.");
        if (DateTime.UtcNow > ExpiresAt)
            throw new DomainException("This quote has expired.");

        Status = QuoteStatus.Converted;
    }

    public void Expire()
    {
        if (Status == QuoteStatus.Converted)
            throw new DomainException("A converted quote cannot be expired.");

        Status = QuoteStatus.Expired;
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
}

public enum QuoteStatus { Active, Converted, Expired }
