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
        {
            var rawConn = config.GetConnectionString("DefaultConnection") ?? "";

            if (rawConn.StartsWith("postgresql://") || rawConn.StartsWith("postgres://"))
            {
                var uri = new Uri(rawConn);
                rawConn = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
                          $"Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};" +
                          $"SSL Mode=Require;Trust Server Certificate=true";
            }

            opts.UseNpgsql(rawConn);
        });

    
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
            client.DefaultRequestHeaders.Add("x-api-key",         config["ClaudeApiKey"]);
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
