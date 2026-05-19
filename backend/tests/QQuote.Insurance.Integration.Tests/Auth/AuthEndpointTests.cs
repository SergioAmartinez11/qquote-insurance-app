using System.Net;
using System.Net.Http.Json;
using QQuote.Insurance.Application.DTOs;

namespace QQuote.Insurance.Integration.Tests.Auth;

public class AuthEndpointTests : IClassFixture<ApiWebAppFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(ApiWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidBody_Returns201WithToken()
    {
        var request = new RegisterRequest(
            "Alice Smith",
            $"{Guid.NewGuid():N}@test.com",
            "SecurePass1!",
            25,
            "90001");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.Equal(request.Email, result.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = $"{Guid.NewGuid():N}@test.com";
        var request = new RegisterRequest("Bob Jones", email, "SecurePass1!", 28, "10001");

        await _client.PostAsJsonAsync("/api/auth/register", request);
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_AgeBelowSixteen_Returns400()
    {
        var request = new RegisterRequest("Teen User", $"{Guid.NewGuid():N}@test.com", "SecurePass1!", 15, "10001");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_CorrectCredentials_Returns200WithToken()
    {
        var email = $"{Guid.NewGuid():N}@test.com";
        const string password = "MyPassword9!";
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Login User", email, password, 30, "10001"));

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = $"{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Pw User", email, "CorrectPass1!", 30, "10001"));

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, "WrongPassword!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("nobody@nowhere.com", "SomePass1!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
