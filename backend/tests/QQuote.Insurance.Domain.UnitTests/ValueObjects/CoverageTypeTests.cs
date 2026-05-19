using QQuote.Insurance.Domain.Exceptions;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Domain.UnitTests.ValueObjects;

public class CoverageTypeTests
{
    [Fact]
    public void From_Basic_ReturnsBasic()
    {
        var result = CoverageType.From("Basic");
        Assert.Equal(CoverageType.Basic, result);
    }

    [Fact]
    public void From_Comprehensive_ReturnsComprehensive()
    {
        var result = CoverageType.From("Comprehensive");
        Assert.Equal(CoverageType.Comprehensive, result);
    }

    [Fact]
    public void From_Limited_ReturnsLimited()
    {
        var result = CoverageType.From("Limited");
        Assert.Equal(CoverageType.Limited, result);
    }

    [Theory]
    [InlineData("basic")]
    [InlineData("BASIC")]
    [InlineData("bAsIc")]
    public void From_IsCaseInsensitive(string name)
    {
        var result = CoverageType.From(name);
        Assert.Equal(CoverageType.Basic, result);
    }

    [Fact]
    public void From_UnknownName_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => CoverageType.From("Unknown"));
    }

    [Fact]
    public void Basic_BaseRateIsFifty()
    {
        Assert.Equal(50m, CoverageType.Basic.BaseRate);
    }

    [Fact]
    public void Comprehensive_BaseRateIsOneTwenty()
    {
        Assert.Equal(120m, CoverageType.Comprehensive.BaseRate);
    }

    [Fact]
    public void Limited_BaseRateIsThirty()
    {
        Assert.Equal(30m, CoverageType.Limited.BaseRate);
    }

    [Fact]
    public void All_ReturnsThreeItems()
    {
        Assert.Equal(3, CoverageType.All().Count);
    }

    [Fact]
    public void Equality_SameType_AreEqual()
    {
        var a = CoverageType.From("Basic");
        var b = CoverageType.From("Basic");
        Assert.Equal(a, b);
    }
}
