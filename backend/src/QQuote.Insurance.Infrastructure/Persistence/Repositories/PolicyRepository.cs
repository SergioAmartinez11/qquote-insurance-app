using Microsoft.EntityFrameworkCore;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Interfaces;
using QQuote.Insurance.Infrastructure.Persistence;

namespace QQuote.Insurance.Infrastructure.Persistence.Repositories;

public class PolicyRepository : IPolicyRepository
{
    private readonly InsuranceDbContext _db;
    public PolicyRepository(InsuranceDbContext db) => _db = db;

    public Task<Policy?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Policies.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<Policy>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct) =>
        _db.Policies.Where(p => p.CustomerId == customerId).ToListAsync(ct);

    public async Task AddAsync(Policy policy, CancellationToken ct) =>
        await _db.Policies.AddAsync(policy, ct);

    public Task UpdateAsync(Policy policy, CancellationToken ct)
    {
        _db.Policies.Update(policy);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        _db.SaveChangesAsync(ct);
}
