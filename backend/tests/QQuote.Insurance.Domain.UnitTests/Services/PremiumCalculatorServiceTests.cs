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

    [Fact]
    public void Calculate_CurrencyIsPassedThrough()
    {
        var vehicle = new Vehicle(DateTime.UtcNow.Year, "Toyota", "Camry");
        var result = _sut.Calculate(CoverageType.Basic, vehicle, RiskLevel.Low, "EUR");
        Assert.Equal("EUR", result.Currency);
    }
}
