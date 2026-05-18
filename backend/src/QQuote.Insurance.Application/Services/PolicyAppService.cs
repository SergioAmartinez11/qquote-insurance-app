using QQuote.Insurance.Application.DTOs;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Exceptions;
using QQuote.Insurance.Domain.Interfaces;

namespace QQuote.Insurance.Application.Services;

public class PolicyAppService
{
    private readonly IQuoteRepository  _quoteRepo;
    private readonly IPolicyRepository _policyRepo;

    public PolicyAppService(IQuoteRepository quoteRepo, IPolicyRepository policyRepo)
    {
        _quoteRepo  = quoteRepo;
        _policyRepo = policyRepo;
    }

    public async Task<List<PolicyResponse>> GetMyPoliciesAsync(
        Guid customerId, CancellationToken ct = default)
    {
        var policies = await _policyRepo.GetByCustomerIdAsync(customerId, ct);
        var result   = new List<PolicyResponse>(policies.Count);

        foreach (var policy in policies)
        {
            var quote = await _quoteRepo.GetByIdAsync(policy.QuoteId, ct);
            if (quote is not null)
                result.Add(MapToResponse(policy, quote));
        }

        return result;
    }

    public async Task<Guid> ConvertAsync(Guid quoteId, CancellationToken ct = default)
    {
        var quote = await _quoteRepo.GetByIdAsync(quoteId, ct)
            ?? throw new NotFoundException(nameof(Quote), quoteId);

        var policy = Policy.CreateFrom(quote);
        quote.ConvertToPolicy();

        await _policyRepo.AddAsync(policy, ct);
        await _quoteRepo.UpdateAsync(quote, ct);
        await _policyRepo.SaveChangesAsync(ct);

        return policy.Id;
    }

    private static PolicyResponse MapToResponse(Policy p, Quote q) => new(
        p.Id,
        p.QuoteId,
        q.Vehicle.Make,
        q.Vehicle.Model,
        q.Vehicle.Year,
        q.CoverageType.Name,
        q.MonthlyPremium.Amount,
        q.MonthlyPremium.Currency,
        p.StartDate,
        p.EndDate,
        p.IsActive);
}
