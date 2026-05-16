using Microsoft.EntityFrameworkCore;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Interfaces;
using QQuote.Insurance.Infrastructure.Persistence;

namespace QQuote.Insurance.Infrastructure.Persistence.Repositories;

public class ClaimRepository : IClaimRepository
{
    private readonly InsuranceDbContext _db;
    public ClaimRepository(InsuranceDbContext db) => _db = db;

    public Task<Claim?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Claims.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<Claim>> GetByPolicyIdAsync(Guid policyId, CancellationToken ct) =>
        _db.Claims.Where(c => c.PolicyId == policyId).ToListAsync(ct);

    public async Task AddAsync(Claim claim, CancellationToken ct) =>
        await _db.Claims.AddAsync(claim, ct);

    public Task UpdateAsync(Claim claim, CancellationToken ct)
    {
        _db.Claims.Update(claim);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        _db.SaveChangesAsync(ct);
}
