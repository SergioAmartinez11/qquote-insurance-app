using System.Net;
using System.Net.Http.Json;
using QQuote.Insurance.Application.DTOs;

namespace QQuote.Insurance.Integration.Tests.Claims;

public class ClaimsEndpointTests : IClassFixture<ApiWebAppFactory>
{
    private readonly ApiWebAppFactory _factory;

    public ClaimsEndpointTests(ApiWebAppFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var token = await TestHelper.RegisterAndGetTokenAsync(client);
        TestHelper.SetBearerToken(client, token);
        return client;
    }

    private async Task<Guid> CreatePolicyAsync(HttpClient client)
    {
        var quoteResp = await client.PostAsJsonAsync("/api/quotes",
            new CreateQuoteRequest("Toyota", "Camry", 2020, "Basic", "USD"));
        quoteResp.EnsureSuccessStatusCode();
        var quote = await quoteResp.Content.ReadFromJsonAsync<QuoteResponse>();

        var convertResp = await client.PostAsJsonAsync(
            $"/api/policies/{quote!.Id}/convert", new { });
        convertResp.EnsureSuccessStatusCode();
        var body = await convertResp.Content.ReadFromJsonAsync<ConvertResult>();
        return body!.PolicyId;
    }

    [Fact]
    public async Task GetClaimsByPolicy_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/claims/policy/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FileClaim_ValidBody_Returns201()
    {
        var client = await CreateAuthenticatedClientAsync();
        var policyId = await CreatePolicyAsync(client);

        var request = new CreateClaimRequest(policyId, DateTime.UtcNow.AddDays(-1), "Windshield cracked.");
        var response = await client.PostAsJsonAsync("/api/claims", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ClaimResponse>();
        Assert.NotNull(result);
        Assert.Equal(policyId, result.PolicyId);
        Assert.Equal("Windshield cracked.", result.Description);
        Assert.Equal("Reported", result.Status);
    }

    [Fact]
    public async Task FileClaim_FutureDate_Returns400()
    {
        var client = await CreateAuthenticatedClientAsync();
        var policyId = await CreatePolicyAsync(client);

        var request = new CreateClaimRequest(policyId, DateTime.UtcNow.AddDays(5), "Future event.");
        var response = await client.PostAsJsonAsync("/api/claims", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetClaimsByPolicy_AfterFilingClaim_ReturnsTheClaim()
    {
        var client = await CreateAuthenticatedClientAsync();
        var policyId = await CreatePolicyAsync(client);

        await client.PostAsJsonAsync("/api/claims",
            new CreateClaimRequest(policyId, DateTime.UtcNow.AddDays(-2), "Rear-end collision."));

        var claims = await client.GetFromJsonAsync<List<ClaimResponse>>(
            $"/api/claims/policy/{policyId}");

        Assert.NotNull(claims);
        Assert.Single(claims);
        Assert.Equal("Rear-end collision.", claims[0].Description);
    }

    private record ConvertResult(Guid PolicyId);
}
