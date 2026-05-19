using System.Reflection;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Exceptions;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Domain.UnitTests.Entities;

public class QuoteTests
{
    private static Quote MakeQuote(Guid? customerId = null)
    {
        var cid = customerId ?? Guid.NewGuid();
        var vehicle = new Vehicle(2020, "Toyota", "Camry");
        return Quote.Create(cid, vehicle, CoverageType.Basic, Money.Of(100m, "USD"), RiskLevel.Low, "Low risk explanation.");
    }

    [Fact]
    public void Create_ValidData_CreatesQuoteWithCorrectProperties()
    {
        var customerId = Guid.NewGuid();
        var vehicle = new Vehicle(2020, "Toyota", "Camry");
        var quote = Quote.Create(customerId, vehicle, CoverageType.Basic, Money.Of(100m, "USD"), RiskLevel.Low, "Low risk.");

        Assert.Equal(customerId, quote.CustomerId);
        Assert.Equal("Toyota", quote.Vehicle.Make);
        Assert.Equal("Camry", quote.Vehicle.Model);
        Assert.Equal(2020, quote.Vehicle.Year);
        Assert.Equal(CoverageType.Basic, quote.CoverageType);
        Assert.Equal(100m, quote.MonthlyPremium.Amount);
        Assert.Equal("USD", quote.MonthlyPremium.Currency);
        Assert.Equal(RiskLevel.Low, quote.RiskLevel);
        Assert.Equal("Low risk.", quote.RiskExplanation);
    }

    [Fact]
    public void Create_EmptyCustomerId_ThrowsDomainException()
    {
        var vehicle = new Vehicle(2020, "Toyota", "Camry");
        Assert.Throws<DomainException>(() =>
            Quote.Create(Guid.Empty, vehicle, CoverageType.Basic, Money.Of(100m, "USD"), RiskLevel.Low, "explanation"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyRiskExplanation_ThrowsDomainException(string explanation)
    {
        var vehicle = new Vehicle(2020, "Toyota", "Camry");
        Assert.Throws<DomainException>(() =>
            Quote.Create(Guid.NewGuid(), vehicle, CoverageType.Basic, Money.Of(100m, "USD"), RiskLevel.Low, explanation));
    }

    [Fact]
    public void Create_StatusIsActiveAfterCreation()
    {
        var quote = MakeQuote();
        Assert.Equal(QuoteStatus.Active, quote.Status);
    }

    [Fact]
    public void Create_ExpiresAtIsApprox30DaysAfterCreatedAt()
    {
        var quote = MakeQuote();
        var diff = quote.ExpiresAt - quote.CreatedAt;
        Assert.True(diff.TotalDays >= 29.9 && diff.TotalDays <= 30.1);
    }

    [Fact]
    public void ConvertToPolicy_ActiveQuote_SetsStatusToConverted()
    {
        var quote = MakeQuote();
        quote.ConvertToPolicy();
        Assert.Equal(QuoteStatus.Converted, quote.Status);
    }

    [Fact]
    public void ConvertToPolicy_NonActiveQuote_ThrowsDomainException()
    {
        var quote = MakeQuote();
        quote.Expire();
        Assert.Throws<DomainException>(() => quote.ConvertToPolicy());
    }

    [Fact]
    public void Expire_ActiveQuote_SetsStatusToExpired()
    {
        var quote = MakeQuote();
        quote.Expire();
        Assert.Equal(QuoteStatus.Expired, quote.Status);
    }

    [Fact]
    public void Expire_ConvertedQuote_ThrowsDomainException()
    {
        var quote = MakeQuote();
        quote.ConvertToPolicy();
        Assert.Throws<DomainException>(() => quote.Expire());
    }

    [Fact]
    public void IsExpired_WhenExpiresAtIsInThePast_ReturnsTrue()
    {
        var quote = MakeQuote();
        var pastDate = DateTime.UtcNow.AddDays(-1);
        typeof(Quote)
            .GetProperty(nameof(Quote.ExpiresAt))!
            .SetValue(quote, pastDate, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);

        Assert.True(quote.IsExpired());
    }
}
