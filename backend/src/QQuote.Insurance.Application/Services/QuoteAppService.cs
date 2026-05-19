using QQuote.Insurance.Application.DTOs;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Exceptions;
using QQuote.Insurance.Domain.Interfaces;
using QQuote.Insurance.Domain.Services;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Application.Services;

public class QuoteAppService
{
    private readonly IQuoteRepository         _quoteRepo;
    private readonly ICustomerRepository      _customerRepo;
    private readonly IClaimRepository         _claimRepo;
    private readonly IRiskAssessmentService   _riskService;
    private readonly PremiumCalculatorService _calculator;

    public QuoteAppService(
        IQuoteRepository        quoteRepo,
        ICustomerRepository     customerRepo,
        IClaimRepository        claimRepo,
        IRiskAssessmentService  riskService,
        PremiumCalculatorService calculator)
    {
        _quoteRepo    = quoteRepo;
        _customerRepo = customerRepo;
        _claimRepo    = claimRepo;
        _riskService  = riskService;
        _calculator   = calculator;
    }

    public async Task<QuoteResponse> CreateAsync(
        Guid               customerId,
        CreateQuoteRequest request,
        CancellationToken  ct = default)
    {
        var customer = await _customerRepo.GetByIdAsync(customerId, ct)
            ?? throw new NotFoundException(nameof(Customer), customerId);

        var vehicle      = new Vehicle(request.VehicleYear, request.VehicleMake, request.VehicleModel);
        var coverageType = CoverageType.From(request.CoverageType);

        var claimsHistory = await _claimRepo.GetByCustomerIdAsync(customerId, ct);
        var riskResult    = await _riskService.AssessAsync(customer, vehicle, claimsHistory, ct);
        var premium    = _calculator.Calculate(coverageType, vehicle, riskResult.Level, request.Currency);

        var quote = Quote.Create(
            customerId,
            vehicle,
            coverageType,
            premium,
            riskResult.Level,
            riskResult.Explanation);

        await _quoteRepo.AddAsync(quote, ct);
        await _quoteRepo.SaveChangesAsync(ct);

        return MapToResponse(quote);
    }

    public async Task<QuoteResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var quote = await _quoteRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Quote), id);

        return MapToResponse(quote);
    }

    public async Task<List<QuoteResponse>> GetByCustomerAsync(
        Guid customerId, CancellationToken ct = default)
    {
        var quotes = await _quoteRepo.GetByCustomerIdAsync(customerId, ct);
        return quotes.Select(MapToResponse).ToList();
    }

    private static QuoteResponse MapToResponse(Quote q) => new(
        q.Id,
        q.Vehicle.Make,
        q.Vehicle.Model,
        q.Vehicle.Year,
        q.CoverageType.Name,
        q.MonthlyPremium.Amount,
        q.MonthlyPremium.Currency,
        q.RiskLevel.Name,
        q.RiskExplanation,
        q.Status.ToString(),
        q.CreatedAt,
        q.ExpiresAt);
}
