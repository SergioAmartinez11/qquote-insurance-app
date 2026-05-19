using System.Net;
using System.Net.Http.Json;
using QQuote.Insurance.Application.DTOs;

namespace QQuote.Insurance.Integration.Tests.Policies;

public class PoliciesEndpointTests : IClassFixture<ApiWebAppFactory>
{
    private readonly ApiWebAppFactory _factory;

    public PoliciesEndpointTests(ApiWebAppFactory factory)
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

    [Fact]
    public async Task GetPolicies_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/policies");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPolicies_Authenticated_ReturnsEmptyListInitially()
    {
        var client = await CreateAuthenticatedClientAsync();
        var result = await client.GetFromJsonAsync<List<PolicyResponse>>("/api/policies");
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ConvertQuote_ValidQuoteId_ReturnsPolicyId()
    {
        var client = await CreateAuthenticatedClientAsync();

        var quoteResponse = await client.PostAsJsonAsync("/api/quotes",
            new CreateQuoteRequest("Honda", "Civic", 2019, "Basic", "USD"));
        quoteResponse.EnsureSuccessStatusCode();
        var quote = await quoteResponse.Content.ReadFromJsonAsync<QuoteResponse>();

        var convertResponse = await client.PostAsJsonAsync(
            $"/api/policies/{quote!.Id}/convert", new { });

        Assert.Equal(HttpStatusCode.OK, convertResponse.StatusCode);
        var body = await convertResponse.Content.ReadFromJsonAsync<ConvertResult>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.PolicyId);
    }

    [Fact]
    public async Task GetPolicies_AfterConversion_ReturnsPolicyInList()
    {
        var client = await CreateAuthenticatedClientAsync();

        var quoteResponse = await client.PostAsJsonAsync("/api/quotes",
            new CreateQuoteRequest("BMW", "3 Series", 2022, "Comprehensive", "USD"));
        quoteResponse.EnsureSuccessStatusCode();
        var quote = await quoteResponse.Content.ReadFromJsonAsync<QuoteResponse>();

        await client.PostAsJsonAsync($"/api/policies/{quote!.Id}/convert", new { });

        var policies = await client.GetFromJsonAsync<List<PolicyResponse>>("/api/policies");
        Assert.NotNull(policies);
        Assert.Single(policies);
        Assert.Equal(quote.Id, policies[0].QuoteId);
    }

    private record ConvertResult(Guid PolicyId);
}
