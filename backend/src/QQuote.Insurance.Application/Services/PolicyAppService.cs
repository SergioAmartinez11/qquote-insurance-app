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
}
