# Insurance Quote API — Clean Architecture Backend

## Stack
- .NET 10 Web API
- Entity Framework Core + SQLite
- JWT Authentication (no MediatR)
- Anthropic Claude API (risk assessment)

---

## Solution structure

```
InsuranceQuote.sln
├── InsuranceQuote.Domain
├── InsuranceQuote.Application
├── InsuranceQuote.Infrastructure
├── InsuranceQuote.API
└── InsuranceQuote.Tests (optional)
```

---

## 1. Create the solution

```bash
mkdir InsuranceQuote && cd InsuranceQuote

dotnet new sln -n InsuranceQuote

dotnet new classlib -n InsuranceQuote.Domain         -o src/InsuranceQuote.Domain
dotnet new classlib -n InsuranceQuote.Application    -o src/InsuranceQuote.Application
dotnet new classlib -n InsuranceQuote.Infrastructure -o src/InsuranceQuote.Infrastructure
dotnet new webapi   -n InsuranceQuote.API            -o src/InsuranceQuote.API

dotnet sln add src/InsuranceQuote.Domain/InsuranceQuote.Domain.csproj
dotnet sln add src/InsuranceQuote.Application/InsuranceQuote.Application.csproj
dotnet sln add src/InsuranceQuote.Infrastructure/InsuranceQuote.Infrastructure.csproj
dotnet sln add src/InsuranceQuote.API/InsuranceQuote.API.csproj
```

---

## 2. Add project references (dependency rule)

```bash
# Application depends on Domain
dotnet add src/InsuranceQuote.Application/InsuranceQuote.Application.csproj \
  reference src/InsuranceQuote.Domain/InsuranceQuote.Domain.csproj

# Infrastructure depends on Domain + Application
dotnet add src/InsuranceQuote.Infrastructure/InsuranceQuote.Infrastructure.csproj \
  reference src/InsuranceQuote.Domain/InsuranceQuote.Domain.csproj

dotnet add src/InsuranceQuote.Infrastructure/InsuranceQuote.Infrastructure.csproj \
  reference src/InsuranceQuote.Application/InsuranceQuote.Application.csproj

# API depends on Application + Infrastructure
dotnet add src/InsuranceQuote.API/InsuranceQuote.API.csproj \
  reference src/InsuranceQuote.Application/InsuranceQuote.Application.csproj

dotnet add src/InsuranceQuote.API/InsuranceQuote.API.csproj \
  reference src/InsuranceQuote.Infrastructure/InsuranceQuote.Infrastructure.csproj
```

---

## 3. Install NuGet packages

```bash
# Infrastructure — EF Core + SQLite
dotnet add src/InsuranceQuote.Infrastructure/InsuranceQuote.Infrastructure.csproj \
  package Microsoft.EntityFrameworkCore
dotnet add src/InsuranceQuote.Infrastructure/InsuranceQuote.Infrastructure.csproj \
  package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/InsuranceQuote.Infrastructure/InsuranceQuote.Infrastructure.csproj \
  package Microsoft.EntityFrameworkCore.Design
dotnet add src/InsuranceQuote.Infrastructure/InsuranceQuote.Infrastructure.csproj \
  package BCrypt.Net-Next

# API — JWT + Swagger
dotnet add src/InsuranceQuote.API/InsuranceQuote.API.csproj \
  package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/InsuranceQuote.API/InsuranceQuote.API.csproj \
  package System.IdentityModel.Tokens.Jwt
dotnet add src/InsuranceQuote.API/InsuranceQuote.API.csproj \
  package Swashbuckle.AspNetCore
dotnet add src/InsuranceQuote.API/InsuranceQuote.API.csproj \
  package Microsoft.EntityFrameworkCore.Design
```

---

## 4. Domain layer

### Folder structure

```
src/InsuranceQuote.Domain/
├── Entities/
│   ├── Quote.cs
│   ├── Customer.cs
│   ├── Policy.cs
│   ├── Claim.cs
│   └── Vehicle.cs
├── ValueObjects/
│   ├── Money.cs
│   ├── RiskLevel.cs
│   ├── CoverageType.cs
│   └── ZipCode.cs
├── Interfaces/
│   ├── IQuoteRepository.cs
│   ├── ICustomerRepository.cs
│   ├── IPolicyRepository.cs
│   ├── IClaimRepository.cs
│   └── IRiskAssessmentService.cs
├── Services/
│   └── PremiumCalculatorService.cs
└── Exceptions/
    └── DomainException.cs
```

### DomainException.cs

```csharp
namespace InsuranceQuote.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entity, object id)
        : base($"{entity} with id '{id}' was not found.") { }
}

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message) : base(message) { }
}
```

### Money.cs

```csharp
namespace InsuranceQuote.Domain.ValueObjects;

public sealed class Money
{
    public decimal Amount   { get; }
    public string  Currency { get; }

    private Money() { }

    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO code.");

        Amount   = Math.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    public static Money Of(decimal amount, string currency) => new(amount, currency);
    public static Money Zero(string currency) => new(0m, currency);

    public Money MultiplyBy(decimal factor)
    {
        if (factor < 0)
            throw new DomainException("Cannot multiply by a negative factor.");
        return new Money(Amount * factor, Currency);
    }

    public static Money operator *(Money a, decimal f) => a.MultiplyBy(f);
    public static Money operator *(decimal f, Money a) => a.MultiplyBy(f);

    public override bool Equals(object? obj) =>
        obj is Money other && Amount == other.Amount && Currency == other.Currency;

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public override string ToString() => $"{Currency} {Amount:F2}";
}
```

### RiskLevel.cs

```csharp
namespace InsuranceQuote.Domain.ValueObjects;

public sealed class RiskLevel
{
    public static readonly RiskLevel Low    = new(nameof(Low),    1.0m);
    public static readonly RiskLevel Medium = new(nameof(Medium), 1.18m);
    public static readonly RiskLevel High   = new(nameof(High),   1.40m);

    public string  Name              { get; }
    public decimal PremiumMultiplier { get; }

    private static readonly IReadOnlyList<RiskLevel> _all = [Low, Medium, High];

    private RiskLevel(string name, decimal multiplier)
    {
        Name              = name;
        PremiumMultiplier = multiplier;
    }

    public static RiskLevel From(string name)
    {
        var level = _all.FirstOrDefault(r =>
            r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (level is null)
            throw new DomainException(
                $"'{name}' is not a valid risk level. Valid: {string.Join(", ", _all.Select(r => r.Name))}");

        return level;
    }

    public static IReadOnlyList<RiskLevel> All() => _all;

    public override bool Equals(object? obj) => obj is RiskLevel other && Name == other.Name;
    public override int GetHashCode() => Name.GetHashCode();
    public static bool operator ==(RiskLevel? a, RiskLevel? b) => a?.Equals(b) ?? b is null;
    public static bool operator !=(RiskLevel? a, RiskLevel? b) => !(a == b);
    public override string ToString() => Name;
}
```

### CoverageType.cs

```csharp
namespace InsuranceQuote.Domain.ValueObjects;

public sealed class CoverageType
{
    public static readonly CoverageType Basic         = new(nameof(Basic),         50m);
    public static readonly CoverageType Comprehensive = new(nameof(Comprehensive), 120m);
    public static readonly CoverageType Limited       = new(nameof(Limited),       30m);

    public string  Name     { get; }
    public decimal BaseRate { get; }

    private static readonly IReadOnlyList<CoverageType> _all = [Basic, Comprehensive, Limited];

    private CoverageType(string name, decimal baseRate)
    {
        Name     = name;
        BaseRate = baseRate;
    }

    public static CoverageType From(string name)
    {
        var type = _all.FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (type is null)
            throw new DomainException(
                $"'{name}' is not a valid coverage type. Valid: {string.Join(", ", _all.Select(c => c.Name))}");

        return type;
    }

    public static IReadOnlyList<CoverageType> All() => _all;

    public override bool Equals(object? obj) => obj is CoverageType other && Name == other.Name;
    public override int GetHashCode() => Name.GetHashCode();
    public static bool operator ==(CoverageType? a, CoverageType? b) => a?.Equals(b) ?? b is null;
    public static bool operator !=(CoverageType? a, CoverageType? b) => !(a == b);
    public override string ToString() => Name;
}
```

### Vehicle.cs (entity, owned by Quote)

```csharp
namespace InsuranceQuote.Domain.Entities;

public sealed class Vehicle
{
    public int    Year  { get; }
    public string Make  { get; }
    public string Model { get; }

    private Vehicle() { }

    public Vehicle(int year, string make, string model)
    {
        if (year < 1900 || year > DateTime.UtcNow.Year + 1)
            throw new DomainException($"Vehicle year {year} is not valid.");
        if (string.IsNullOrWhiteSpace(make))
            throw new DomainException("Vehicle make is required.");
        if (string.IsNullOrWhiteSpace(model))
            throw new DomainException("Vehicle model is required.");

        Year  = year;
        Make  = make.Trim();
        Model = model.Trim();
    }

    public int Age() => DateTime.UtcNow.Year - Year;
}
```

### Quote.cs

```csharp
namespace InsuranceQuote.Domain.Entities;

public class Quote
{
    public Guid         Id              { get; private set; }
    public Guid         CustomerId      { get; private set; }
    public Vehicle      Vehicle         { get; private set; } = null!;
    public CoverageType CoverageType    { get; private set; } = null!;
    public Money        MonthlyPremium  { get; private set; } = null!;
    public RiskLevel    RiskLevel       { get; private set; } = null!;
    public string       RiskExplanation { get; private set; } = string.Empty;
    public QuoteStatus  Status          { get; private set; }
    public DateTime     CreatedAt       { get; private set; }
    public DateTime     ExpiresAt       { get; private set; }

    private Quote() { }

    public static Quote Create(
        Guid         customerId,
        Vehicle      vehicle,
        CoverageType coverageType,
        Money        monthlyPremium,
        RiskLevel    riskLevel,
        string       riskExplanation)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("CustomerId is required.");
        if (string.IsNullOrWhiteSpace(riskExplanation))
            throw new DomainException("Risk explanation is required.");

        return new Quote
        {
            Id              = Guid.NewGuid(),
            CustomerId      = customerId,
            Vehicle         = vehicle,
            CoverageType    = coverageType,
            MonthlyPremium  = monthlyPremium,
            RiskLevel       = riskLevel,
            RiskExplanation = riskExplanation,
            Status          = QuoteStatus.Active,
            CreatedAt       = DateTime.UtcNow,
            ExpiresAt       = DateTime.UtcNow.AddDays(30)
        };
    }

    public void ConvertToPolicy()
    {
        if (Status != QuoteStatus.Active)
            throw new DomainException("Only active quotes can be converted.");
        if (DateTime.UtcNow > ExpiresAt)
            throw new DomainException("This quote has expired.");

        Status = QuoteStatus.Converted;
    }

    public void Expire()
    {
        if (Status == QuoteStatus.Converted)
            throw new DomainException("A converted quote cannot be expired.");

        Status = QuoteStatus.Expired;
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
}

public enum QuoteStatus { Active, Converted, Expired }
```

### Customer.cs

```csharp
namespace InsuranceQuote.Domain.Entities;

public class Customer
{
    public Guid   Id           { get; private set; }
    public string FullName     { get; private set; } = string.Empty;
    public string Email        { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public int    Age          { get; private set; }
    public string ZipCode      { get; private set; } = string.Empty;
    public int    PriorClaims  { get; private set; }
    public DateTime CreatedAt  { get; private set; }

    private Customer() { }

    public static Customer Create(
        string fullName,
        string email,
        string passwordHash,
        int    age,
        string zipCode)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("A valid email is required.");
        if (age < 16 || age > 100)
            throw new DomainException("Age must be between 16 and 100.");

        return new Customer
        {
            Id           = Guid.NewGuid(),
            FullName     = fullName.Trim(),
            Email        = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            Age          = age,
            ZipCode      = zipCode.Trim(),
            PriorClaims  = 0,
            CreatedAt    = DateTime.UtcNow
        };
    }
}
```

### Policy.cs

```csharp
namespace InsuranceQuote.Domain.Entities;

public class Policy
{
    public Guid      Id        { get; private set; }
    public Guid      QuoteId   { get; private set; }
    public Guid      CustomerId{ get; private set; }
    public DateTime  StartDate { get; private set; }
    public DateTime  EndDate   { get; private set; }
    public bool      IsActive  { get; private set; }

    private Policy() { }

    public static Policy CreateFrom(Quote quote)
    {
        if (quote.Status != QuoteStatus.Active)
            throw new DomainException("Cannot create a policy from a non-active quote.");

        return new Policy
        {
            Id         = Guid.NewGuid(),
            QuoteId    = quote.Id,
            CustomerId = quote.CustomerId,
            StartDate  = DateTime.UtcNow,
            EndDate    = DateTime.UtcNow.AddYears(1),
            IsActive   = true
        };
    }

    public void Cancel()
    {
        if (!IsActive)
            throw new DomainException("Policy is already inactive.");

        IsActive = false;
    }
}
```

### Claim.cs

```csharp
namespace InsuranceQuote.Domain.Entities;

public class Claim
{
    public Guid        Id          { get; private set; }
    public Guid        PolicyId    { get; private set; }
    public DateTime    IncidentDate{ get; private set; }
    public string      Description { get; private set; } = string.Empty;
    public ClaimStatus Status      { get; private set; }
    public DateTime    CreatedAt   { get; private set; }

    private Claim() { }

    public static Claim File(Guid policyId, DateTime incidentDate, string description)
    {
        if (policyId == Guid.Empty)
            throw new DomainException("PolicyId is required.");
        if (incidentDate > DateTime.UtcNow)
            throw new DomainException("Incident date cannot be in the future.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");

        return new Claim
        {
            Id           = Guid.NewGuid(),
            PolicyId     = policyId,
            IncidentDate = incidentDate,
            Description  = description.Trim(),
            Status       = ClaimStatus.Reported,
            CreatedAt    = DateTime.UtcNow
        };
    }

    public void Resolve() => Status = ClaimStatus.Resolved;
    public void StartReview() => Status = ClaimStatus.UnderReview;
}

public enum ClaimStatus { Reported, UnderReview, Resolved }
```

### Repository interfaces

```csharp
// Domain/Interfaces/IQuoteRepository.cs
namespace InsuranceQuote.Domain.Interfaces;

public interface IQuoteRepository
{
    Task<Quote?>       GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Quote>>  GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task               AddAsync(Quote quote, CancellationToken ct = default);
    Task               UpdateAsync(Quote quote, CancellationToken ct = default);
    Task               SaveChangesAsync(CancellationToken ct = default);
}

// Domain/Interfaces/ICustomerRepository.cs
public interface ICustomerRepository
{
    Task<Customer?>   GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Customer?>   GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool>        ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task              AddAsync(Customer customer, CancellationToken ct = default);
    Task              SaveChangesAsync(CancellationToken ct = default);
}

// Domain/Interfaces/IPolicyRepository.cs
public interface IPolicyRepository
{
    Task<Policy?>     GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Policy>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task              AddAsync(Policy policy, CancellationToken ct = default);
    Task              UpdateAsync(Policy policy, CancellationToken ct = default);
    Task              SaveChangesAsync(CancellationToken ct = default);
}

// Domain/Interfaces/IClaimRepository.cs
public interface IClaimRepository
{
    Task<Claim?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Claim>> GetByPolicyIdAsync(Guid policyId, CancellationToken ct = default);
    Task              AddAsync(Claim claim, CancellationToken ct = default);
    Task              UpdateAsync(Claim claim, CancellationToken ct = default);
    Task              SaveChangesAsync(CancellationToken ct = default);
}

// Domain/Interfaces/IRiskAssessmentService.cs
public interface IRiskAssessmentService
{
    Task<RiskAssessmentResult> AssessAsync(
        Customer customer,
        Vehicle  vehicle,
        CancellationToken ct = default);
}

public record RiskAssessmentResult(RiskLevel Level, string Explanation);
```

### PremiumCalculatorService.cs

```csharp
namespace InsuranceQuote.Domain.Services;

public class PremiumCalculatorService
{
    public Money Calculate(
        CoverageType coverage,
        Vehicle      vehicle,
        RiskLevel    riskLevel,
        string       currency = "USD")
    {
        var vehicleAgeFactor = 1m + (vehicle.Age() * 0.02m);
        var basePremium      = Money.Of(coverage.BaseRate, currency);

        return basePremium * vehicleAgeFactor * riskLevel.PremiumMultiplier;
    }
}
```

---

## 5. Application layer

### Folder structure

```
src/InsuranceQuote.Application/
├── Interfaces/
│   ├── IJwtTokenService.cs
│   ├── IPasswordHasher.cs
│   └── ICurrentUserService.cs
├── DTOs/
│   ├── QuoteDto.cs
│   ├── AuthDto.cs
│   ├── CustomerDto.cs
│   ├── PolicyDto.cs
│   └── ClaimDto.cs
└── Services/
    ├── QuoteAppService.cs
    ├── AuthAppService.cs
    ├── PolicyAppService.cs
    └── ClaimAppService.cs
```

### Application interfaces

```csharp
// Application/Interfaces/IJwtTokenService.cs
namespace InsuranceQuote.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(Customer customer);
}

// Application/Interfaces/IPasswordHasher.cs
public interface IPasswordHasher
{
    string Hash(string password);
    bool   Verify(string password, string hash);
}

// Application/Interfaces/ICurrentUserService.cs
public interface ICurrentUserService
{
    Guid   CustomerId { get; }
    string Email      { get; }
}
```

### DTOs

```csharp
// Application/DTOs/QuoteDto.cs
namespace InsuranceQuote.Application.DTOs;

public record CreateQuoteRequest(
    string VehicleMake,
    string VehicleModel,
    int    VehicleYear,
    string CoverageType,
    string Currency = "USD");

public record QuoteResponse(
    Guid    Id,
    string  VehicleMake,
    string  VehicleModel,
    int     VehicleYear,
    string  CoverageType,
    decimal MonthlyPremium,
    string  Currency,
    string  RiskLevel,
    string  RiskExplanation,
    string  Status,
    DateTime CreatedAt,
    DateTime ExpiresAt);

// Application/DTOs/AuthDto.cs
public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    int    Age,
    string ZipCode);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    string FullName,
    string Email,
    DateTime ExpiresAt);

// Application/DTOs/ClaimDto.cs
public record CreateClaimRequest(
    Guid     PolicyId,
    DateTime IncidentDate,
    string   Description);

public record ClaimResponse(
    Guid     Id,
    Guid     PolicyId,
    DateTime IncidentDate,
    string   Description,
    string   Status,
    DateTime CreatedAt);
```

### QuoteAppService.cs

```csharp
namespace InsuranceQuote.Application.Services;

public class QuoteAppService
{
    private readonly IQuoteRepository        _quoteRepo;
    private readonly ICustomerRepository     _customerRepo;
    private readonly IRiskAssessmentService  _riskService;
    private readonly PremiumCalculatorService _calculator;

    public QuoteAppService(
        IQuoteRepository       quoteRepo,
        ICustomerRepository    customerRepo,
        IRiskAssessmentService riskService,
        PremiumCalculatorService calculator)
    {
        _quoteRepo    = quoteRepo;
        _customerRepo = customerRepo;
        _riskService  = riskService;
        _calculator   = calculator;
    }

    public async Task<QuoteResponse> CreateAsync(
        Guid               customerId,
        CreateQuoteRequest request,
        CancellationToken  ct = default)
    {
        var customer = await _customerRepo.GetByIdAsync(customerId, ct)
            ?? throw new NotFoundException(nameof(Customer), customerId);

        var vehicle      = new Vehicle(request.VehicleYear, request.VehicleMake, request.VehicleModel);
        var coverageType = CoverageType.From(request.CoverageType);

        var riskResult   = await _riskService.AssessAsync(customer, vehicle, ct);
        var premium      = _calculator.Calculate(coverageType, vehicle, riskResult.Level, request.Currency);

        var quote = Quote.Create(
            customerId,
            vehicle,
            coverageType,
            premium,
            riskResult.Level,
            riskResult.Explanation);

        await _quoteRepo.AddAsync(quote, ct);
        await _quoteRepo.SaveChangesAsync(ct);

        return MapToResponse(quote);
    }

    public async Task<QuoteResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var quote = await _quoteRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Quote), id);

        return MapToResponse(quote);
    }

    public async Task<List<QuoteResponse>> GetByCustomerAsync(
        Guid customerId, CancellationToken ct = default)
    {
        var quotes = await _quoteRepo.GetByCustomerIdAsync(customerId, ct);
        return quotes.Select(MapToResponse).ToList();
    }

    private static QuoteResponse MapToResponse(Quote q) => new(
        q.Id,
        q.Vehicle.Make,
        q.Vehicle.Model,
        q.Vehicle.Year,
        q.CoverageType.Name,
        q.MonthlyPremium.Amount,
        q.MonthlyPremium.Currency,
        q.RiskLevel.Name,
        q.RiskExplanation,
        q.Status.ToString(),
        q.CreatedAt,
        q.ExpiresAt);
}
```

### AuthAppService.cs

```csharp
namespace InsuranceQuote.Application.Services;

public class AuthAppService
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IPasswordHasher     _passwordHasher;
    private readonly IJwtTokenService    _jwtTokenService;

    public AuthAppService(
        ICustomerRepository customerRepo,
        IPasswordHasher     passwordHasher,
        IJwtTokenService    jwtTokenService)
    {
        _customerRepo    = customerRepo;
        _passwordHasher  = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest   request,
        CancellationToken ct = default)
    {
        if (await _customerRepo.ExistsByEmailAsync(request.Email, ct))
            throw new ConflictException("Email already registered.");

        var hash     = _passwordHasher.Hash(request.Password);
        var customer = Customer.Create(request.FullName, request.Email, hash, request.Age, request.ZipCode);

        await _customerRepo.AddAsync(customer, ct);
        await _customerRepo.SaveChangesAsync(ct);

        var token = _jwtTokenService.GenerateToken(customer);
        return new AuthResponse(token, customer.FullName, customer.Email, DateTime.UtcNow.AddHours(1));
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest      request,
        CancellationToken ct = default)
    {
        var customer = await _customerRepo.GetByEmailAsync(request.Email, ct)
            ?? throw new UnauthorizedException("Invalid credentials.");

        if (!_passwordHasher.Verify(request.Password, customer.PasswordHash))
            throw new UnauthorizedException("Invalid credentials.");

        var token = _jwtTokenService.GenerateToken(customer);
        return new AuthResponse(token, customer.FullName, customer.Email, DateTime.UtcNow.AddHours(1));
    }
}
```

### PolicyAppService.cs

```csharp
namespace InsuranceQuote.Application.Services;

public class PolicyAppService
{
    private readonly IQuoteRepository  _quoteRepo;
    private readonly IPolicyRepository _policyRepo;

    public PolicyAppService(IQuoteRepository quoteRepo, IPolicyRepository policyRepo)
    {
        _quoteRepo  = quoteRepo;
        _policyRepo = policyRepo;
    }

    public async Task<Guid> ConvertAsync(Guid quoteId, CancellationToken ct = default)
    {
        var quote = await _quoteRepo.GetByIdAsync(quoteId, ct)
            ?? throw new NotFoundException(nameof(Quote), quoteId);

        var policy = Policy.CreateFrom(quote);
        quote.ConvertToPolicy();

        await _policyRepo.AddAsync(policy, ct);
        await _quoteRepo.UpdateAsync(quote, ct);
        await _policyRepo.SaveChangesAsync(ct);

        return policy.Id;
    }
}
```

### ClaimAppService.cs

```csharp
namespace InsuranceQuote.Application.Services;

public class ClaimAppService
{
    private readonly IClaimRepository _claimRepo;

    public ClaimAppService(IClaimRepository claimRepo) => _claimRepo = claimRepo;

    public async Task<ClaimResponse> FileAsync(
        CreateClaimRequest request,
        CancellationToken  ct = default)
    {
        var claim = Claim.File(request.PolicyId, request.IncidentDate, request.Description);
        await _claimRepo.AddAsync(claim, ct);
        await _claimRepo.SaveChangesAsync(ct);
        return MapToResponse(claim);
    }

    public async Task<List<ClaimResponse>> GetByPolicyAsync(
        Guid policyId, CancellationToken ct = default)
    {
        var claims = await _claimRepo.GetByPolicyIdAsync(policyId, ct);
        return claims.Select(MapToResponse).ToList();
    }

    private static ClaimResponse MapToResponse(Claim c) => new(
        c.Id, c.PolicyId, c.IncidentDate, c.Description, c.Status.ToString(), c.CreatedAt);
}
```

---

## 6. Infrastructure layer

### Folder structure

```
src/InsuranceQuote.Infrastructure/
├── Persistence/
│   ├── InsuranceDbContext.cs
│   ├── Configurations/
│   │   ├── QuoteConfiguration.cs
│   │   └── CustomerConfiguration.cs
│   ├── QuoteRepository.cs
│   ├── CustomerRepository.cs
│   ├── PolicyRepository.cs
│   └── ClaimRepository.cs
├── ExternalServices/
│   ├── ClaudeRiskAssessmentService.cs
│   └── RiskPromptBuilder.cs
├── Identity/
│   ├── JwtTokenService.cs
│   ├── JwtSettings.cs
│   ├── PasswordHasher.cs
│   └── CurrentUserService.cs
└── DependencyInjection/
    └── ServiceExtensions.cs
```

### InsuranceDbContext.cs

```csharp
namespace InsuranceQuote.Infrastructure.Persistence;

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
```

### QuoteConfiguration.cs

```csharp
namespace InsuranceQuote.Infrastructure.Persistence.Configurations;

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.HasKey(q => q.Id);

        builder.OwnsOne(q => q.Vehicle, v =>
        {
            v.Property(x => x.Year).HasColumnName("VehicleYear").IsRequired();
            v.Property(x => x.Make).HasColumnName("VehicleMake").HasMaxLength(100).IsRequired();
            v.Property(x => x.Model).HasColumnName("VehicleModel").HasMaxLength(100).IsRequired();
        });

        builder.OwnsOne(q => q.MonthlyPremium, m =>
        {
            m.Property(x => x.Amount).HasColumnName("PremiumAmount")
             .HasColumnType("decimal(18,2)").IsRequired();
            m.Property(x => x.Currency).HasColumnName("PremiumCurrency")
             .HasMaxLength(3).IsRequired();
        });

        builder.Property(q => q.CoverageType)
               .HasConversion(c => c.Name, name => CoverageType.From(name))
               .HasMaxLength(50).IsRequired();

        builder.Property(q => q.RiskLevel)
               .HasConversion(r => r.Name, name => RiskLevel.From(name))
               .HasMaxLength(20).IsRequired();

        builder.Property(q => q.RiskExplanation).HasMaxLength(500).IsRequired();
        builder.Property(q => q.Status).HasConversion<string>().HasMaxLength(20);
    }
}
```

### CustomerConfiguration.cs

```csharp
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.Email).IsUnique();
        builder.Property(c => c.FullName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(200).IsRequired();
        builder.Property(c => c.PasswordHash).IsRequired();
    }
}
```

### QuoteRepository.cs

```csharp
namespace InsuranceQuote.Infrastructure.Persistence;

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
```

### CustomerRepository.cs

```csharp
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
```

### JwtSettings.cs + JwtTokenService.cs

```csharp
// Identity/JwtSettings.cs
namespace InsuranceQuote.Infrastructure.Identity;

public class JwtSettings
{
    public string SecretKey          { get; set; } = string.Empty;
    public string Issuer             { get; set; } = string.Empty;
    public string Audience           { get; set; } = string.Empty;
    public int    ExpirationMinutes  { get; set; } = 60;
}

// Identity/JwtTokenService.cs
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings         _settings;
    private readonly SymmetricSecurityKey _key;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
        _key      = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
    }

    public string GenerateToken(Customer customer)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new Claim(ClaimTypes.Email,          customer.Email),
            new Claim(ClaimTypes.Name,           customer.FullName),
        };

        var token = new JwtSecurityToken(
            issuer:             _settings.Issuer,
            audience:           _settings.Audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### PasswordHasher.cs

```csharp
namespace InsuranceQuote.Infrastructure.Identity;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)   => BCrypt.Net.BCrypt.HashPassword(password);
    public bool   Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
```

### CurrentUserService.cs

```csharp
namespace InsuranceQuote.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    public Guid   CustomerId { get; }
    public string Email      { get; }

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User;
        CustomerId = Guid.Parse(user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
        Email      = user?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    }
}
```

### ClaudeRiskAssessmentService.cs

```csharp
namespace InsuranceQuote.Infrastructure.ExternalServices;

public class ClaudeRiskAssessmentService : IRiskAssessmentService
{
    private readonly HttpClient _http;

    public ClaudeRiskAssessmentService(HttpClient http) => _http = http;

    public async Task<RiskAssessmentResult> AssessAsync(
        Customer customer, Vehicle vehicle, CancellationToken ct = default)
    {
        var prompt = RiskPromptBuilder.Build(customer, vehicle);

        var body = new
        {
            model      = "claude-sonnet-4-20250514",
            max_tokens = 300,
            messages   = new[] { new { role = "user", content = prompt } }
        };

        var response = await _http.PostAsJsonAsync(
            "https://api.anthropic.com/v1/messages", body, ct);

        response.EnsureSuccessStatusCode();

        var result  = await response.Content.ReadFromJsonAsync<ClaudeResponse>(cancellationToken: ct);
        var text    = result?.Content?.FirstOrDefault()?.Text ?? string.Empty;

        return ParseResponse(text);
    }

    private static RiskAssessmentResult ParseResponse(string text)
    {
        var lower = text.ToLowerInvariant();
        var level = lower.Contains("high")   ? RiskLevel.High   :
                    lower.Contains("medium") ? RiskLevel.Medium  :
                                               RiskLevel.Low;
        return new RiskAssessmentResult(level, text.Trim());
    }

    private record ClaudeResponse(List<ContentBlock>? Content);
    private record ContentBlock(string Text);
}
```

### RiskPromptBuilder.cs

```csharp
namespace InsuranceQuote.Infrastructure.ExternalServices;

public static class RiskPromptBuilder
{
    public static string Build(Customer customer, Vehicle vehicle) =>
        $"""
        You are an insurance actuary. Evaluate this driver profile and respond with:
        1. Risk level: exactly one word — Low, Medium, or High
        2. One sentence explanation for the customer

        Driver: {customer.FullName}, age {customer.Age}, zip {customer.ZipCode}
        Vehicle: {vehicle.Year} {vehicle.Make} {vehicle.Model} (age: {vehicle.Age()} years)
        Prior claims: {customer.PriorClaims}

        Format: "LEVEL: [level]. [explanation sentence]"
        """;
}
```

### ServiceExtensions.cs

```csharp
namespace InsuranceQuote.Infrastructure.DependencyInjection;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration          config)
    {
        // Database
        services.AddDbContext<InsuranceDbContext>(opts =>
            opts.UseSqlite(config.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IQuoteRepository,    QuoteRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPolicyRepository,   PolicyRepository>();
        services.AddScoped<IClaimRepository,    ClaimRepository>();

        // Identity
        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
        services.AddScoped<IJwtTokenService,   JwtTokenService>();
        services.AddScoped<IPasswordHasher,    PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();

        // AI service
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
```

---

## 7. API layer

### Middleware/GlobalExceptionMiddleware.cs

```csharp
namespace InsuranceQuote.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (NotFoundException ex)
        {
            ctx.Response.StatusCode = 404;
            await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            ctx.Response.StatusCode = 401;
            await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (ConflictException ex)
        {
            ctx.Response.StatusCode = 409;
            await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            ctx.Response.StatusCode = 400;
            await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            ctx.Response.StatusCode = 500;
            await ctx.Response.WriteAsJsonAsync(new { error = "Unexpected error.", detail = ex.Message });
        }
    }
}
```

### Controllers/AuthController.cs

```csharp
namespace InsuranceQuote.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthAppService _auth;
    public AuthController(AuthAppService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _auth.RegisterAsync(request, ct);
        return Created("/api/auth/me", result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(request, ct);
        return Ok(result);
    }
}
```

### Controllers/QuotesController.cs

```csharp
[ApiController]
[Authorize]
[Route("api/quotes")]
public class QuotesController : ControllerBase
{
    private readonly QuoteAppService    _quotes;
    private readonly ICurrentUserService _currentUser;

    public QuotesController(QuoteAppService quotes, ICurrentUserService currentUser)
    {
        _quotes      = quotes;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateQuoteRequest request, CancellationToken ct)
    {
        var result = await _quotes.CreateAsync(_currentUser.CustomerId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _quotes.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyQuotes(CancellationToken ct)
    {
        var result = await _quotes.GetByCustomerAsync(_currentUser.CustomerId, ct);
        return Ok(result);
    }
}
```

### Controllers/PoliciesController.cs

```csharp
[ApiController]
[Authorize]
[Route("api/policies")]
public class PoliciesController : ControllerBase
{
    private readonly PolicyAppService _policies;
    public PoliciesController(PolicyAppService policies) => _policies = policies;

    [HttpPost("{quoteId:guid}/convert")]
    public async Task<IActionResult> Convert(Guid quoteId, CancellationToken ct)
    {
        var policyId = await _policies.ConvertAsync(quoteId, ct);
        return Ok(new { policyId });
    }
}
```

### Controllers/ClaimsController.cs

```csharp
[ApiController]
[Authorize]
[Route("api/claims")]
public class ClaimsController : ControllerBase
{
    private readonly ClaimAppService _claims;
    public ClaimsController(ClaimAppService claims) => _claims = claims;

    [HttpPost]
    public async Task<IActionResult> File(
        [FromBody] CreateClaimRequest request, CancellationToken ct)
    {
        var result = await _claims.FileAsync(request, ct);
        return Created($"/api/claims/{result.Id}", result);
    }

    [HttpGet("policy/{policyId:guid}")]
    public async Task<IActionResult> GetByPolicy(Guid policyId, CancellationToken ct)
    {
        var result = await _claims.GetByPolicyAsync(policyId, ct);
        return Ok(result);
    }
}
```

### Program.cs

```csharp
using InsuranceQuote.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings["Issuer"],
            ValidAudience            = jwtSettings["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-migrate on startup
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
await db.Database.MigrateAsync();

app.Run();
```

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=insurance.db"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-minimum-32-chars!!",
    "Issuer": "InsuranceQuoteApi",
    "Audience": "InsuranceQuoteClient",
    "ExpirationMinutes": 60
  },
  "Claude": {
    "ApiKey": "YOUR_ANTHROPIC_API_KEY"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 8. Run EF Core migrations

```bash
cd src/InsuranceQuote.API

dotnet ef migrations add InitialCreate \
  --project ../InsuranceQuote.Infrastructure/InsuranceQuote.Infrastructure.csproj \
  --startup-project InsuranceQuote.API.csproj

dotnet ef database update \
  --project ../InsuranceQuote.Infrastructure/InsuranceQuote.Infrastructure.csproj \
  --startup-project InsuranceQuote.API.csproj
```

---

## 9. Run the API

```bash
dotnet run --project src/InsuranceQuote.API/InsuranceQuote.API.csproj
```

Swagger UI available at: `https://localhost:5001/swagger`

---

## 10. Quick test sequence

```bash
# 1. Register
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Sergio Test","email":"sergio@test.com","password":"Pass123!","age":28,"zipCode":"22000"}'

# 2. Login — copy the token from response
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"sergio@test.com","password":"Pass123!"}'

# 3. Create a quote (replace TOKEN)
curl -X POST https://localhost:5001/api/quotes \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"vehicleMake":"Honda","vehicleModel":"Civic","vehicleYear":2019,"coverageType":"Comprehensive","currency":"USD"}'

# 4. Get my quotes
curl https://localhost:5001/api/quotes \
  -H "Authorization: Bearer TOKEN"
```
