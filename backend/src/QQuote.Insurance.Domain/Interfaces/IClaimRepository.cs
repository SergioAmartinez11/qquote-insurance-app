using QQuote.Insurance.Domain.Entities;

namespace QQuote.Insurance.Domain.Interfaces;

public interface IClaimRepository
{
    Task<Claim?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Claim>> GetByPolicyIdAsync(Guid policyId, CancellationToken ct = default);
    Task              AddAsync(Claim claim, CancellationToken ct = default);
    Task              UpdateAsync(Claim claim, CancellationToken ct = default);
    Task              SaveChangesAsync(CancellationToken ct = default);
}
