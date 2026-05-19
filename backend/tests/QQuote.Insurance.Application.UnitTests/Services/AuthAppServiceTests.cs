using NSubstitute;
using QQuote.Insurance.Application.Common.Interfaces;
using QQuote.Insurance.Application.DTOs;
using QQuote.Insurance.Application.Services;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Exceptions;
using QQuote.Insurance.Domain.Interfaces;

namespace QQuote.Insurance.Application.UnitTests.Services;

public class AuthAppServiceTests
{
    private readonly ICustomerRepository _customerRepo = Substitute.For<ICustomerRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly AuthAppService _sut;

    public AuthAppServiceTests()
    {
        _sut = new AuthAppService(_customerRepo, _passwordHasher, _jwtTokenService);
    }

    [Fact]
    public async Task RegisterAsync_UniqueEmail_CreatesCustomerAndReturnsToken()
    {
        _customerRepo.ExistsByEmailAsync("new@example.com").Returns(false);
        _passwordHasher.Hash("password123").Returns("hashed");
        _jwtTokenService.GenerateToken(Arg.Any<Customer>()).Returns("jwt-token");

        var request = new RegisterRequest("New User", "new@example.com", "password123", 30, "10001");
        var result = await _sut.RegisterAsync(request);

        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("new@example.com", result.Email);
        await _customerRepo.Received(1).AddAsync(Arg.Any<Customer>());
        await _customerRepo.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task RegisterAsync_ExistingEmail_ThrowsConflictException()
    {
        _customerRepo.ExistsByEmailAsync("existing@example.com").Returns(true);

        var request = new RegisterRequest("Existing User", "existing@example.com", "password123", 30, "10001");

        await Assert.ThrowsAsync<ConflictException>(() => _sut.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_CorrectCredentials_ReturnsToken()
    {
        var customer = Customer.Create("John Doe", "john@example.com", "hashed", 30, "10001");
        _customerRepo.GetByEmailAsync("john@example.com").Returns(customer);
        _passwordHasher.Verify("password123", "hashed").Returns(true);
        _jwtTokenService.GenerateToken(customer).Returns("jwt-token");

        var request = new LoginRequest("john@example.com", "password123");
        var result = await _sut.LoginAsync(request);

        Assert.Equal("jwt-token", result.Token);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsUnauthorizedException()
    {
        _customerRepo.GetByEmailAsync("unknown@example.com").Returns((Customer?)null);

        var request = new LoginRequest("unknown@example.com", "password123");

        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        var customer = Customer.Create("John Doe", "john@example.com", "hashed", 30, "10001");
        _customerRepo.GetByEmailAsync("john@example.com").Returns(customer);
        _passwordHasher.Verify("wrongpassword", "hashed").Returns(false);

        var request = new LoginRequest("john@example.com", "wrongpassword");

        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task GoogleSignInAsync_ExistingCustomer_ReturnsTokenWithoutCreating()
    {
        var existing = Customer.CreateFromGoogle("Jane Doe", "jane@example.com");
        _customerRepo.GetByEmailAsync("jane@example.com").Returns(existing);
        _jwtTokenService.GenerateToken(existing).Returns("google-token");

        var result = await _sut.GoogleSignInAsync("jane@example.com", "Jane Doe");

        Assert.Equal("google-token", result.Token);
        await _customerRepo.DidNotReceive().AddAsync(Arg.Any<Customer>());
    }

    [Fact]
    public async Task GoogleSignInAsync_NewEmail_CreatesCustomerAndReturnsToken()
    {
        _customerRepo.GetByEmailAsync("newgoogle@example.com").Returns((Customer?)null);
        _jwtTokenService.GenerateToken(Arg.Any<Customer>()).Returns("google-token");

        var result = await _sut.GoogleSignInAsync("newgoogle@example.com", "New Google User");

        Assert.Equal("google-token", result.Token);
        await _customerRepo.Received(1).AddAsync(Arg.Any<Customer>());
        await _customerRepo.Received(1).SaveChangesAsync();
    }
}
