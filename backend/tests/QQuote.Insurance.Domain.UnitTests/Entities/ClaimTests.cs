using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Exceptions;

namespace QQuote.Insurance.Domain.UnitTests.Entities;

public class ClaimTests
{
    [Fact]
    public void File_ValidData_CreatesClaim()
    {
        var policyId = Guid.NewGuid();
        var claim = Claim.File(policyId, DateTime.UtcNow.AddDays(-1), "Windshield cracked.");

        Assert.Equal(policyId, claim.PolicyId);
        Assert.Equal("Windshield cracked.", claim.Description);
        Assert.NotEqual(Guid.Empty, claim.Id);
    }

    [Fact]
    public void File_EmptyPolicyId_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Claim.File(Guid.Empty, DateTime.UtcNow.AddDays(-1), "Description."));
    }

    [Fact]
    public void File_FutureIncidentDate_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Claim.File(Guid.NewGuid(), DateTime.UtcNow.AddDays(1), "Description."));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void File_EmptyDescription_ThrowsDomainException(string description)
    {
        Assert.Throws<DomainException>(() =>
            Claim.File(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), description));
    }

    [Fact]
    public void File_DescriptionIsTrimmed()
    {
        var claim = Claim.File(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), "  Windshield cracked.  ");
        Assert.Equal("Windshield cracked.", claim.Description);
    }

    [Fact]
    public void File_StatusIsReportedAfterFiling()
    {
        var claim = Claim.File(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), "Windshield cracked.");
        Assert.Equal(ClaimStatus.Reported, claim.Status);
    }

    [Fact]
    public void Resolve_SetsStatusToResolved()
    {
        var claim = Claim.File(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), "Windshield cracked.");
        claim.Resolve();
        Assert.Equal(ClaimStatus.Resolved, claim.Status);
    }

    [Fact]
    public void StartReview_SetsStatusToUnderReview()
    {
        var claim = Claim.File(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), "Windshield cracked.");
        claim.StartReview();
        Assert.Equal(ClaimStatus.UnderReview, claim.Status);
    }
}
