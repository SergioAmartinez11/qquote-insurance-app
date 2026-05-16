using QQuote.Insurance.Domain.Entities;

namespace QQuote.Insurance.Domain.Interfaces;

public interface IPolicyRepository
{
    Task<Policy?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Policy>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task               AddAsync(Policy policy, CancellationToken ct = default);
    Task               UpdateAsync(Policy policy, CancellationToken ct = default);
    Task               SaveChangesAsync(CancellationToken ct = default);
}
