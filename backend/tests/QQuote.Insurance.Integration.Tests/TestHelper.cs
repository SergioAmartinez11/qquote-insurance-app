using System.Net.Http.Json;
using QQuote.Insurance.Application.DTOs;

namespace QQuote.Insurance.Integration.Tests;

public static class TestHelper
{
    public static async Task<string> RegisterAndGetTokenAsync(
        HttpClient client,
        string? email = null,
        string password = "Test@12345",
        string fullName = "Test User",
        int age = 30,
        string zipCode = "10001")
    {
        email ??= $"{Guid.NewGuid():N}@test.com";

        var registerRequest = new RegisterRequest(fullName, email, password, age, zipCode);
        var response = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result!.Token;
    }

    public static async Task<string> LoginAndGetTokenAsync(
        HttpClient client,
        string email,
        string password = "Test@12345")
    {
        var loginRequest = new LoginRequest(email, password);
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result!.Token;
    }

    public static void SetBearerToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
