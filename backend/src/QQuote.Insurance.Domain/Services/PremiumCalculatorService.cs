using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Domain.Services;

public class PremiumCalculatorService
{
    public Money Calculate(
        CoverageType coverage,
        Vehicle      vehicle,
        RiskLevel    riskLevel,
        string       currency = "USD")
    {
        var vehicleAgeFactor = 1m + (vehicle.Age() * 0.02m);
        var basePremium      = Money.Of(coverage.BaseRate, currency);

        return basePremium * vehicleAgeFactor * riskLevel.PremiumMultiplier;
    }
}
