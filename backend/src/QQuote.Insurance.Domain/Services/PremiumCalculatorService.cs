using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Domain.Services;

public class PremiumCalculatorService
{
    private static readonly Dictionary<string, decimal> ExchangeRates =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["USD"] = 1.00m,
            ["EUR"] = 0.92m,
            ["MXN"] = 17.15m,
            ["GBP"] = 0.79m,
        };

    public Money Calculate(
        CoverageType coverage,
        Vehicle      vehicle,
        RiskLevel    riskLevel,
        string       currency = "USD")
    {
        var vehicleAgeFactor = 1m + (vehicle.Age() * 0.02m);
        var amountUsd        = coverage.BaseRate * vehicleAgeFactor * riskLevel.PremiumMultiplier;

        var rate = ExchangeRates.GetValueOrDefault(currency.ToUpperInvariant(), 1.00m);
        return Money.Of(Math.Round(amountUsd * rate, 2), currency);
    }
}
