using Microsoft.EntityFrameworkCore;
using QQuote.Insurance.Domain.Entities;

namespace QQuote.Insurance.Infrastructure.Persistence;

public class InsuranceDbContext : DbContext
{
    public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options)
        : base(options) { }

    public DbSet<Quote>    Quotes    => Set<Quote>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Policy>   Policies  => Set<Policy>();
    public DbSet<Claim>    Claims    => Set<Claim>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InsuranceDbContext).Assembly);
    }
}
