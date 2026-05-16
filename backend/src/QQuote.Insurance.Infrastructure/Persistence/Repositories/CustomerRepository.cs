using Microsoft.EntityFrameworkCore;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Interfaces;
using QQuote.Insurance.Infrastructure.Persistence;

namespace QQuote.Insurance.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly InsuranceDbContext _db;
    public CustomerRepository(InsuranceDbContext db) => _db = db;

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Customer?> GetByEmailAsync(string email, CancellationToken ct) =>
        _db.Customers.FirstOrDefaultAsync(c => c.Email == email.ToLowerInvariant(), ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        _db.Customers.AnyAsync(c => c.Email == email.ToLowerInvariant(), ct);

    public async Task AddAsync(Customer customer, CancellationToken ct) =>
        await _db.Customers.AddAsync(customer, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        _db.SaveChangesAsync(ct);
}
