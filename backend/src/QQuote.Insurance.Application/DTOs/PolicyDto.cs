namespace QQuote.Insurance.Application.DTOs;

public record PolicyResponse(
    Guid     Id,
    Guid     QuoteId,
    string   VehicleMake,
    string   VehicleModel,
    int      VehicleYear,
    string   CoverageType,
    decimal  MonthlyPremium,
    string   Currency,
    DateTime StartDate,
    DateTime EndDate,
    bool     IsActive);
