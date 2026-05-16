using Microsoft.EntityFrameworkCore;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Interfaces;
using QQuote.Insurance.Infrastructure.Persistence;

namespace QQuote.Insurance.Infrastructure.Persistence.Repositories;

public class QuoteRepository : IQuoteRepository
{
    private readonly InsuranceDbContext _db;
    public QuoteRepository(InsuranceDbContext db) => _db = db;

    public Task<Quote?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Quotes.FirstOrDefaultAsync(q => q.Id == id, ct);

    public Task<List<Quote>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct) =>
        _db.Quotes.Where(q => q.CustomerId == customerId).ToListAsync(ct);

    public async Task AddAsync(Quote quote, CancellationToken ct) =>
        await _db.Quotes.AddAsync(quote, ct);

    public Task UpdateAsync(Quote quote, CancellationToken ct)
    {
        _db.Quotes.Update(quote);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        _db.SaveChangesAsync(ct);
}
