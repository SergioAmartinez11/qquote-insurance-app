using System.Net;
using System.Net.Http.Json;
using QQuote.Insurance.Application.DTOs;

namespace QQuote.Insurance.Integration.Tests.Quotes;

public class QuotesEndpointTests : IClassFixture<ApiWebAppFactory>
{
    private readonly ApiWebAppFactory _factory;

    public QuotesEndpointTests(ApiWebAppFactory factory)
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
    public async Task GetQuotes_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/quotes");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostQuote_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();
        var request = new CreateQuoteRequest("Toyota", "Camry", 2020, "Basic", "USD");
        var response = await client.PostAsJsonAsync("/api/quotes", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostQuote_ValidBody_Returns201WithQuoteData()
    {
        var client = await CreateAuthenticatedClientAsync();
        var request = new CreateQuoteRequest("Toyota", "Camry", 2020, "Basic", "USD");

        var response = await client.PostAsJsonAsync("/api/quotes", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<QuoteResponse>();
        Assert.NotNull(result);
        Assert.Equal("Toyota", result.VehicleMake);
        Assert.Equal("Camry", result.VehicleModel);
        Assert.Equal(2020, result.VehicleYear);
        Assert.Equal("Basic", result.CoverageType);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task PostQuote_InvalidCoverageType_Returns400()
    {
        var client = await CreateAuthenticatedClientAsync();
        var request = new CreateQuoteRequest("Toyota", "Camry", 2020, "Platinum", "USD");

        var response = await client.PostAsJsonAsync("/api/quotes", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetQuotes_AfterCreatingQuote_ReturnsListWithItem()
    {
        var client = await CreateAuthenticatedClientAsync();

        var emptyResponse = await client.GetFromJsonAsync<List<QuoteResponse>>("/api/quotes");
        Assert.NotNull(emptyResponse);
        Assert.Empty(emptyResponse);

        await client.PostAsJsonAsync("/api/quotes",
            new CreateQuoteRequest("Ford", "F-150", 2021, "Comprehensive", "USD"));

        var listResponse = await client.GetFromJsonAsync<List<QuoteResponse>>("/api/quotes");
        Assert.NotNull(listResponse);
        Assert.Single(listResponse);
    }
}
