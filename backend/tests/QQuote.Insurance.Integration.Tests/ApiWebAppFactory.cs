using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Interfaces;
using QQuote.Insurance.Domain.ValueObjects;
using QQuote.Insurance.Infrastructure.Persistence;
using QQuote.Insurance.Infrastructure.Services;

namespace QQuote.Insurance.Integration.Tests;

public class ApiWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var dbName = "TestDb_" + Guid.NewGuid();

        builder.ConfigureServices(services =>
        {
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<InsuranceDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>)) ||
                    d.ImplementationType == typeof(ClaudeRiskAssessmentService) ||
                    d.ServiceType == typeof(IRiskAssessmentService))
                .ToList();

            foreach (var desc in descriptorsToRemove)
                services.Remove(desc);

            services.AddDbContext<InsuranceDbContext>(opts =>
                opts.UseInMemoryDatabase(dbName));

            services.AddSingleton<IRiskAssessmentService, FakeRiskAssessmentService>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<InsuranceDbContext>().Database.EnsureCreated();
        return host;
    }
}

internal class FakeRiskAssessmentService : IRiskAssessmentService
{
    public Task<RiskAssessmentResult> AssessAsync(
        Customer             customer,
        Vehicle              vehicle,
        IReadOnlyList<Claim> claimsHistory,
        CancellationToken    ct = default) =>
        Task.FromResult(new RiskAssessmentResult(RiskLevel.Low, "Low risk."));
}
