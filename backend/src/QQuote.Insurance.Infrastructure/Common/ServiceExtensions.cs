using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QQuote.Insurance.Application.Common.Interfaces;
using QQuote.Insurance.Application.Services;
using QQuote.Insurance.Domain.Interfaces;
using QQuote.Insurance.Domain.Services;
using QQuote.Insurance.Infrastructure.Identity;
using QQuote.Insurance.Infrastructure.Persistence;
using QQuote.Insurance.Infrastructure.Persistence.Repositories;
using QQuote.Insurance.Infrastructure.Services;

namespace QQuote.Insurance.Infrastructure.Common;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration          config)
    {
        services.AddDbContext<InsuranceDbContext>(opts =>
            opts.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IQuoteRepository,    QuoteRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPolicyRepository,   PolicyRepository>();
        services.AddScoped<IClaimRepository,    ClaimRepository>();

        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
        services.AddScoped<IJwtTokenService,    JwtTokenService>();
        services.AddScoped<IPasswordHasher,     PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();

        services.AddHttpClient<IRiskAssessmentService, ClaudeRiskAssessmentService>(client =>
        {
            client.DefaultRequestHeaders.Add("x-api-key",         config["Claude:ApiKey"]);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        });

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<QuoteAppService>();
        services.AddScoped<AuthAppService>();
        services.AddScoped<PolicyAppService>();
        services.AddScoped<ClaimAppService>();
        services.AddScoped<PremiumCalculatorService>();
        return services;
    }
}
