using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Exceptions;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Domain.UnitTests.Entities;

public class PolicyTests
{
    private static Quote MakeActiveQuote(Guid? customerId = null)
    {
        var cid = customerId ?? Guid.NewGuid();
        var vehicle = new Vehicle(2020, "Toyota", "Camry");
        return Quote.Create(cid, vehicle, CoverageType.Basic, Money.Of(100m, "USD"), RiskLevel.Low, "Low risk.");
    }

    [Fact]
    public void CreateFrom_ActiveQuote_CreatesPolicyCorrectly()
    {
        var quote = MakeActiveQuote();
        var policy = Policy.CreateFrom(quote);

        Assert.Equal(quote.Id, policy.QuoteId);
        Assert.Equal(quote.CustomerId, policy.CustomerId);
        Assert.NotEqual(Guid.Empty, policy.Id);
    }

    [Fact]
    public void CreateFrom_NonActiveQuote_ThrowsDomainException()
    {
        var quote = MakeActiveQuote();
        quote.Expire();
        Assert.Throws<DomainException>(() => Policy.CreateFrom(quote));
    }

    [Fact]
    public void IsActive_IsTrueAfterCreation()
    {
        var quote = MakeActiveQuote();
        var policy = Policy.CreateFrom(quote);
        Assert.True(policy.IsActive);
    }

    [Fact]
    public void CreateFrom_PolicyQuoteIdAndCustomerIdMatchQuote()
    {
        var customerId = Guid.NewGuid();
        var quote = MakeActiveQuote(customerId);
        var policy = Policy.CreateFrom(quote);

        Assert.Equal(quote.Id, policy.QuoteId);
        Assert.Equal(customerId, policy.CustomerId);
    }

    [Fact]
    public void Cancel_ActivePolicy_SetsIsActiveToFalse()
    {
        var quote = MakeActiveQuote();
        var policy = Policy.CreateFrom(quote);
        policy.Cancel();
        Assert.False(policy.IsActive);
    }

    [Fact]
    public void Cancel_AlreadyInactivePolicy_ThrowsDomainException()
    {
        var quote = MakeActiveQuote();
        var policy = Policy.CreateFrom(quote);
        policy.Cancel();
        Assert.Throws<DomainException>(() => policy.Cancel());
    }

    [Fact]
    public void CreateFrom_StartDateAndEndDateSetCorrectly()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var quote = MakeActiveQuote();
        var policy = Policy.CreateFrom(quote);
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.True(policy.StartDate >= before && policy.StartDate <= after);
        var expectedEnd = policy.StartDate.AddYears(1);
        Assert.True(Math.Abs((policy.EndDate - expectedEnd).TotalSeconds) < 5);
    }
}
