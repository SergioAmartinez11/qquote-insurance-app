using QQuote.Insurance.Application.Common.Interfaces;
using QQuote.Insurance.Application.DTOs;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Exceptions;
using QQuote.Insurance.Domain.Interfaces;

namespace QQuote.Insurance.Application.Services;

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

    public async Task<AuthResponse> GoogleSignInAsync(
        string            email,
        string            fullName,
        CancellationToken ct = default)
    {
        var customer = await _customerRepo.GetByEmailAsync(email, ct);
        if (customer is null)
        {
            customer = Customer.CreateFromGoogle(fullName, email);
            await _customerRepo.AddAsync(customer, ct);
            await _customerRepo.SaveChangesAsync(ct);
        }

        var token = _jwtTokenService.GenerateToken(customer);
        return new AuthResponse(token, customer.FullName, customer.Email, DateTime.UtcNow.AddHours(1));
    }
}
