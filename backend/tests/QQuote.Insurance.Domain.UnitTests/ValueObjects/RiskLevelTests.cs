using QQuote.Insurance.Domain.Exceptions;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Domain.UnitTests.ValueObjects;

public class RiskLevelTests
{
    [Fact]
    public void From_Low_ReturnsLow()
    {
        Assert.Equal(RiskLevel.Low, RiskLevel.From("Low"));
    }

    [Fact]
    public void From_Medium_ReturnsMedium()
    {
        Assert.Equal(RiskLevel.Medium, RiskLevel.From("Medium"));
    }

    [Fact]
    public void From_High_ReturnsHigh()
    {
        Assert.Equal(RiskLevel.High, RiskLevel.From("High"));
    }

    [Theory]
    [InlineData("low")]
    [InlineData("LOW")]
    [InlineData("LoW")]
    public void From_IsCaseInsensitive(string name)
    {
        Assert.Equal(RiskLevel.Low, RiskLevel.From(name));
    }

    [Fact]
    public void From_UnknownName_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => RiskLevel.From("VeryHigh"));
    }

    [Fact]
    public void Low_PremiumMultiplierIsOnePointZero()
    {
        Assert.Equal(1.0m, RiskLevel.Low.PremiumMultiplier);
    }

    [Fact]
    public void Medium_PremiumMultiplierIsOnePointEighteen()
    {
        Assert.Equal(1.18m, RiskLevel.Medium.PremiumMultiplier);
    }

    [Fact]
    public void High_PremiumMultiplierIsOnePointForty()
    {
        Assert.Equal(1.40m, RiskLevel.High.PremiumMultiplier);
    }

    [Fact]
    public void Equality_SameLevel_AreEqual()
    {
        var a = RiskLevel.From("Low");
        var b = RiskLevel.From("Low");
        Assert.Equal(a, b);
    }
}
