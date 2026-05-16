using QQuote.Insurance.Domain.Exceptions;

namespace QQuote.Insurance.Domain.Entities;

public class Policy
{
    public Guid     Id         { get; private set; }
    public Guid     QuoteId    { get; private set; }
    public Guid     CustomerId { get; private set; }
    public DateTime StartDate  { get; private set; }
    public DateTime EndDate    { get; private set; }
    public bool     IsActive   { get; private set; }

    private Policy() { }

    public static Policy CreateFrom(Quote quote)
    {
        if (quote.Status != QuoteStatus.Active)
            throw new DomainException("Cannot create a policy from a non-active quote.");

        return new Policy
        {
            Id         = Guid.NewGuid(),
            QuoteId    = quote.Id,
            CustomerId = quote.CustomerId,
            StartDate  = DateTime.UtcNow,
            EndDate    = DateTime.UtcNow.AddYears(1),
            IsActive   = true
        };
    }

    public void Cancel()
    {
        if (!IsActive)
            throw new DomainException("Policy is already inactive.");

        IsActive = false;
    }
}
