namespace QQuote.Insurance.Application.DTOs;

public record CreateQuoteRequest(
    string VehicleMake,
    string VehicleModel,
    int    VehicleYear,
    string CoverageType,
    string Currency = "USD");

public record QuoteResponse(
    Guid     Id,
    string   VehicleMake,
    string   VehicleModel,
    int      VehicleYear,
    string   CoverageType,
    decimal  MonthlyPremium,
    string   Currency,
    string   RiskLevel,
    string   RiskExplanation,
    string   Status,
    DateTime CreatedAt,
    DateTime ExpiresAt);
