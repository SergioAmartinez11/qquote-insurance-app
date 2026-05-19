using NSubstitute;
using QQuote.Insurance.Application.DTOs;
using QQuote.Insurance.Application.Services;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Exceptions;
using QQuote.Insurance.Domain.Interfaces;
using QQuote.Insurance.Domain.Services;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Application.UnitTests.Services;

public class QuoteAppServiceTests
{
    private readonly IQuoteRepository _quoteRepo = Substitute.For<IQuoteRepository>();
    private readonly ICustomerRepository _customerRepo = Substitute.For<ICustomerRepository>();
    private readonly IClaimRepository _claimRepo = Substitute.For<IClaimRepository>();
    private readonly IRiskAssessmentService _riskService = Substitute.For<IRiskAssessmentService>();
    private readonly PremiumCalculatorService _calculator = new();
    private readonly QuoteAppService _sut;

    public QuoteAppServiceTests()
    {
        _claimRepo.GetByCustomerIdAsync(Arg.Any<Guid>()).Returns(new List<Claim>());
        _sut = new QuoteAppService(_quoteRepo, _customerRepo, _claimRepo, _riskService, _calculator);
    }

    [Fact]
    public async Task CreateAsync_UnknownCustomerId_ThrowsNotFoundException()
    {
        var unknownId = Guid.NewGuid();
        _customerRepo.GetByIdAsync(unknownId).Returns((Customer?)null);

        var request = new CreateQuoteRequest("Toyota", "Camry", 2020, "Basic", "USD");

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(unknownId, request));
    }

    [Fact]
    public async Task CreateAsync_ValidData_CallsRiskServiceAndCalculatorAndReturnsResponse()
    {
        var customerId = Guid.NewGuid();
        var customer = Customer.Create("John Doe", "john@example.com", "hash", 30, "10001");
        _customerRepo.GetByIdAsync(customerId).Returns(customer);
        _riskService.AssessAsync(customer, Arg.Any<Vehicle>(), Arg.Any<IReadOnlyList<Claim>>())
            .Returns(new RiskAssessmentResult(RiskLevel.Low, "Low risk."));

        var request = new CreateQuoteRequest("Toyota", "Camry", 2020, "Basic", "USD");
        var result = await _sut.CreateAsync(customerId, request);

        Assert.Equal("Toyota", result.VehicleMake);
        Assert.Equal("Camry", result.VehicleModel);
        Assert.Equal(2020, result.VehicleYear);
        Assert.Equal("Basic", result.CoverageType);
        Assert.Equal("Low", result.RiskLevel);
        Assert.Equal("Low risk.", result.RiskExplanation);
        await _riskService.Received(1).AssessAsync(customer, Arg.Any<Vehicle>(), Arg.Any<IReadOnlyList<Claim>>());
        await _quoteRepo.Received(1).AddAsync(Arg.Any<Quote>());
        await _quoteRepo.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ThrowsNotFoundException()
    {
        var unknownId = Guid.NewGuid();
        _quoteRepo.GetByIdAsync(unknownId).Returns((Quote?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(unknownId));
    }

    [Fact]
    public async Task GetByIdAsync_KnownId_ReturnsMappedResponse()
    {
        var customerId = Guid.NewGuid();
        var vehicle = new Vehicle(2020, "Honda", "Civic");
        var quote = Quote.Create(customerId, vehicle, CoverageType.Comprehensive, Money.Of(150m, "USD"), RiskLevel.Medium, "Medium risk.");
        _quoteRepo.GetByIdAsync(quote.Id).Returns(quote);

        var result = await _sut.GetByIdAsync(quote.Id);

        Assert.Equal(quote.Id, result.Id);
        Assert.Equal("Honda", result.VehicleMake);
        Assert.Equal("Comprehensive", result.CoverageType);
        Assert.Equal("Medium", result.RiskLevel);
    }

    [Fact]
    public async Task CreateAsync_FetchesClaimsForCustomerAndForwardsToRiskService()
    {
        var customerId = Guid.NewGuid();
        var customer   = Customer.Create("Jane Doe", "jane@example.com", "hash", 35, "90210");
        var claims     = new List<Claim>
        {
            Claim.File(Guid.NewGuid(), DateTime.UtcNow.AddDays(-30),  "Rear-end collision."),
            Claim.File(Guid.NewGuid(), DateTime.UtcNow.AddDays(-120), "Windshield crack."),
        };
        _customerRepo.GetByIdAsync(customerId).Returns(customer);
        _claimRepo.GetByCustomerIdAsync(customerId).Returns(claims);
        _riskService.AssessAsync(customer, Arg.Any<Vehicle>(), Arg.Any<IReadOnlyList<Claim>>())
            .Returns(new RiskAssessmentResult(RiskLevel.High, "High risk."));

        await _sut.CreateAsync(customerId, new CreateQuoteRequest("Ford", "Mustang", 2020, "Comprehensive", "USD"));

        await _claimRepo.Received(1).GetByCustomerIdAsync(customerId);
        await _riskService.Received(1).AssessAsync(
            customer,
            Arg.Any<Vehicle>(),
            Arg.Is<IReadOnlyList<Claim>>(c => c.Count == 2));
    }

    [Fact]
    public async Task GetByCustomerAsync_NoQuotes_ReturnsEmptyList()
    {
        var customerId = Guid.NewGuid();
        _quoteRepo.GetByCustomerIdAsync(customerId).Returns(new List<Quote>());

        var result = await _sut.GetByCustomerAsync(customerId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByCustomerAsync_HasQuotes_ReturnsMappedList()
    {
        var customerId = Guid.NewGuid();
        var vehicle = new Vehicle(2022, "Ford", "F-150");
        var quote1 = Quote.Create(customerId, vehicle, CoverageType.Basic, Money.Of(80m, "USD"), RiskLevel.Low, "Low.");
        var quote2 = Quote.Create(customerId, vehicle, CoverageType.Limited, Money.Of(50m, "USD"), RiskLevel.High, "High.");
        _quoteRepo.GetByCustomerIdAsync(customerId).Returns(new List<Quote> { quote1, quote2 });

        var result = await _sut.GetByCustomerAsync(customerId);

        Assert.Equal(2, result.Count);
    }
}
