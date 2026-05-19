using NSubstitute;
using QQuote.Insurance.Application.DTOs;
using QQuote.Insurance.Application.Services;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Exceptions;
using QQuote.Insurance.Domain.Interfaces;

namespace QQuote.Insurance.Application.UnitTests.Services;

public class ClaimAppServiceTests
{
    private readonly IClaimRepository _claimRepo = Substitute.For<IClaimRepository>();
    private readonly ClaimAppService _sut;

    public ClaimAppServiceTests()
    {
        _sut = new ClaimAppService(_claimRepo);
    }

    [Fact]
    public async Task FileAsync_ValidRequest_CreatesClaimAndReturnsResponse()
    {
        var policyId = Guid.NewGuid();
        var request = new CreateClaimRequest(policyId, DateTime.UtcNow.AddDays(-1), "Fender bender.");

        var result = await _sut.FileAsync(request);

        Assert.Equal(policyId, result.PolicyId);
        Assert.Equal("Fender bender.", result.Description);
        Assert.Equal("Reported", result.Status);
        await _claimRepo.Received(1).AddAsync(Arg.Any<Claim>());
        await _claimRepo.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task FileAsync_FutureIncidentDate_ThrowsDomainException()
    {
        var request = new CreateClaimRequest(Guid.NewGuid(), DateTime.UtcNow.AddDays(1), "Future event.");

        await Assert.ThrowsAsync<DomainException>(() => _sut.FileAsync(request));
    }

    [Fact]
    public async Task GetByPolicyAsync_NoClaims_ReturnsEmptyList()
    {
        var policyId = Guid.NewGuid();
        _claimRepo.GetByPolicyIdAsync(policyId).Returns(new List<Claim>());

        var result = await _sut.GetByPolicyAsync(policyId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByPolicyAsync_HasClaims_ReturnsMappedList()
    {
        var policyId = Guid.NewGuid();
        var claim1 = Claim.File(policyId, DateTime.UtcNow.AddDays(-1), "Windshield cracked.");
        var claim2 = Claim.File(policyId, DateTime.UtcNow.AddDays(-2), "Tire puncture.");
        _claimRepo.GetByPolicyIdAsync(policyId).Returns(new List<Claim> { claim1, claim2 });

        var result = await _sut.GetByPolicyAsync(policyId);

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(policyId, r.PolicyId));
    }
}
