using QQuote.Insurance.Domain.Exceptions;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Domain.UnitTests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Of_CreatesMoneyWithCorrectAmountAndCurrency()
    {
        var money = Money.Of(99.99m, "USD");
        Assert.Equal(99.99m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Of_NegativeAmount_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Money.Of(-1m, "USD"));
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("")]
    [InlineData("   ")]
    public void Of_InvalidCurrency_ThrowsDomainException(string currency)
    {
        Assert.Throws<DomainException>(() => Money.Of(10m, currency));
    }

    [Fact]
    public void Of_CurrencyIsStoredUppercase()
    {
        var money = Money.Of(10m, "usd");
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Of_AmountIsRoundedToTwoDecimalPlaces()
    {
        var money = Money.Of(10.555m, "USD");
        Assert.Equal(10.56m, money.Amount);
    }

    [Fact]
    public void Zero_CreatesMoneyWithZeroAmount()
    {
        var money = Money.Zero("USD");
        Assert.Equal(0m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void MultiplyBy_PositiveFactor_ReturnsCorrectResult()
    {
        var money = Money.Of(100m, "USD");
        var result = money.MultiplyBy(1.5m);
        Assert.Equal(150m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void MultiplyBy_NegativeFactor_ThrowsDomainException()
    {
        var money = Money.Of(100m, "USD");
        Assert.Throws<DomainException>(() => money.MultiplyBy(-1m));
    }

    [Fact]
    public void MultiplyOperator_WorksCorrectly()
    {
        var money = Money.Of(50m, "USD");
        var result = money * 2m;
        Assert.Equal(100m, result.Amount);
    }

    [Fact]
    public void Equality_SameAmountAndCurrency_AreEqual()
    {
        var a = Money.Of(100m, "USD");
        var b = Money.Of(100m, "USD");
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentCurrencies_AreNotEqual()
    {
        var a = Money.Of(100m, "USD");
        var b = Money.Of(100m, "EUR");
        Assert.NotEqual(a, b);
    }
}
