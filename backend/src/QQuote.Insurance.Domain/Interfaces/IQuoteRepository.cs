using QQuote.Insurance.Domain.Entities;

namespace QQuote.Insurance.Domain.Interfaces;

public interface IQuoteRepository
{
    Task<Quote?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Quote>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task              AddAsync(Quote quote, CancellationToken ct = default);
    Task              UpdateAsync(Quote quote, CancellationToken ct = default);
    Task              SaveChangesAsync(CancellationToken ct = default);
}
