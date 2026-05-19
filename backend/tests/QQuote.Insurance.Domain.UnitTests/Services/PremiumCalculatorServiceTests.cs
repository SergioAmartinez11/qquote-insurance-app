using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Services;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Domain.UnitTests.Services;

public class PremiumCalculatorServiceTests
{
    private readonly PremiumCalculatorService _sut = new();

    [Fact]
    public void Calculate_BasicCoverageAndLowRisk_ReturnsCorrectPremium()
    {
        var vehicle = new Vehicle(DateTime.UtcNow.Year, "Toyota", "Camry");
        var result = _sut.Calculate(CoverageType.Basic, vehicle, RiskLevel.Low, "USD");

        var vehicleAgeFactor = 1m + (vehicle.Age() * 0.02m);
        var expected = Math.Round(50m * vehicleAgeFactor * 1.0m, 2);
        Assert.Equal(expected, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Calculate_ComprehensiveCoverageAndHighRisk_ReturnsCorrectPremium()
    {
        var vehicle = new Vehicle(DateTime.UtcNow.Year, "BMW", "M3");
        var result = _sut.Calculate(CoverageType.Comprehensive, vehicle, RiskLevel.High, "USD");

        var vehicleAgeFactor = 1m + (vehicle.Age() * 0.02m);
        var expected = Math.Round(120m * vehicleAgeFactor * 1.40m, 2);
        Assert.Equal(expected, result.Amount);
    }

    [Fact]
    public void Calculate_OlderVehicle_HasHigherPremiumDueToAgeFactor()
    {
        var newVehicle = new Vehicle(DateTime.UtcNow.Year, "Toyota", "Camry");
        var oldVehicle = new Vehicle(DateTime.UtcNow.Year - 10, "Toyota", "Camry");

        var newPremium = _sut.Calculate(CoverageType.Basic, newVehicle, RiskLevel.Low, "USD");
        var oldPremium = _sut.Calculate(CoverageType.Basic, oldVehicle, RiskLevel.Low, "USD");

        Assert.True(oldPremium.Amount > newPremium.Amount);
    }

    [Theory]
    [InlineData("USD", 1.00)]
    [InlineData("EUR", 0.92)]
    [InlineData("MXN", 17.15)]
    [InlineData("GBP", 0.79)]
    public void Calculate_AppliesExchangeRateToUsdBaseAmount(string currency, double rate)
    {
        var vehicle = new Vehicle(DateTime.UtcNow.Year, "Toyota", "Camry");
        var result = _sut.Calculate(CoverageType.Basic, vehicle, RiskLevel.Low, currency);

        var vehicleAgeFactor = 1m + (vehicle.Age() * 0.02m);
        var expected = Math.Round(50m * vehicleAgeFactor * 1.0m * (decimal)rate, 2);

        Assert.Equal(expected, result.Amount);
        Assert.Equal(currency, result.Currency);
    }

    [Fact]
    public void Calculate_UnknownCurrency_DefaultsToUsdRate()
    {
        var vehicle = new Vehicle(DateTime.UtcNow.Year, "Toyota", "Camry");
        var usd     = _sut.Calculate(CoverageType.Basic, vehicle, RiskLevel.Low, "USD");
        var unknown = _sut.Calculate(CoverageType.Basic, vehicle, RiskLevel.Low, "CHF");

        Assert.Equal(usd.Amount, unknown.Amount);
        Assert.Equal("CHF", unknown.Currency);
    }

    [Fact]
    public void Calculate_MxnPremium_IsSignificantlyHigherThanUsd()
    {
        var vehicle = new Vehicle(DateTime.UtcNow.Year, "Toyota", "Camry");
        var usd = _sut.Calculate(CoverageType.Basic, vehicle, RiskLevel.Low, "USD");
        var mxn = _sut.Calculate(CoverageType.Basic, vehicle, RiskLevel.Low, "MXN");

        Assert.True(mxn.Amount > usd.Amount * 10);
    }
}
