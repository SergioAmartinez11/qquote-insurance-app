using NSubstitute;
using QQuote.Insurance.Application.Services;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Exceptions;
using QQuote.Insurance.Domain.Interfaces;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Application.UnitTests.Services;

public class PolicyAppServiceTests
{
    private readonly IQuoteRepository _quoteRepo = Substitute.For<IQuoteRepository>();
    private readonly IPolicyRepository _policyRepo = Substitute.For<IPolicyRepository>();
    private readonly PolicyAppService _sut;

    public PolicyAppServiceTests()
    {
        _sut = new PolicyAppService(_quoteRepo, _policyRepo);
    }

    private static Quote MakeActiveQuote(Guid? customerId = null)
    {
        var cid = customerId ?? Guid.NewGuid();
        var vehicle = new Vehicle(2020, "Toyota", "Camry");
        return Quote.Create(cid, vehicle, CoverageType.Basic, Money.Of(100m, "USD"), RiskLevel.Low, "Low risk.");
    }

    [Fact]
    public async Task ConvertAsync_UnknownQuoteId_ThrowsNotFoundException()
    {
        var unknownId = Guid.NewGuid();
        _quoteRepo.GetByIdAsync(unknownId).Returns((Quote?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ConvertAsync(unknownId));
    }

    [Fact]
    public async Task ConvertAsync_ValidQuoteId_CreatesPolicyAndUpdatesQuote()
    {
        var quote = MakeActiveQuote();
        _quoteRepo.GetByIdAsync(quote.Id).Returns(quote);

        var policyId = await _sut.ConvertAsync(quote.Id);

        Assert.NotEqual(Guid.Empty, policyId);
        await _policyRepo.Received(1).AddAsync(Arg.Any<Policy>());
        await _quoteRepo.Received(1).UpdateAsync(quote);
        await _policyRepo.Received(1).SaveChangesAsync();
        Assert.Equal(QuoteStatus.Converted, quote.Status);
    }

    [Fact]
    public async Task GetMyPoliciesAsync_NoPolicies_ReturnsEmptyList()
    {
        var customerId = Guid.NewGuid();
        _policyRepo.GetByCustomerIdAsync(customerId).Returns(new List<Policy>());

        var result = await _sut.GetMyPoliciesAsync(customerId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMyPoliciesAsync_HasPolicies_ReturnsMappedList()
    {
        var customerId = Guid.NewGuid();
        var quote = MakeActiveQuote(customerId);
        var policy = Policy.CreateFrom(quote);

        _policyRepo.GetByCustomerIdAsync(customerId).Returns(new List<Policy> { policy });
        _quoteRepo.GetByIdAsync(quote.Id).Returns(quote);

        var result = await _sut.GetMyPoliciesAsync(customerId);

        Assert.Single(result);
        Assert.Equal(policy.Id, result[0].Id);
    }

    [Fact]
    public async Task GetMyPoliciesAsync_PolicyWithMissingQuote_SkipsIt()
    {
        var customerId = Guid.NewGuid();
        var quote = MakeActiveQuote(customerId);
        var policy = Policy.CreateFrom(quote);

        _policyRepo.GetByCustomerIdAsync(customerId).Returns(new List<Policy> { policy });
        _quoteRepo.GetByIdAsync(policy.QuoteId).Returns((Quote?)null);

        var result = await _sut.GetMyPoliciesAsync(customerId);

        Assert.Empty(result);
    }
}
