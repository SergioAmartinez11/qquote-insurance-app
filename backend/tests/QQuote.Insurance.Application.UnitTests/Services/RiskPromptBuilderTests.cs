using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Infrastructure.Services;

namespace QQuote.Insurance.Application.UnitTests.Services;

public class RiskPromptBuilderTests
{
    private readonly Customer _customer = Customer.Create("Alice Smith", "alice@example.com", "hash", 28, "90001");
    private readonly Vehicle  _vehicle  = new(2019, "Honda", "Civic");

    [Fact]
    public void Build_NoClaimHistory_ContainsNoneSection()
    {
        var prompt = RiskPromptBuilder.Build(_customer, _vehicle, []);

        Assert.Contains("Prior claims: 0", prompt);
        Assert.Contains("Claim history: none", prompt);
    }

    [Fact]
    public void Build_WithClaims_ContainsEachClaimLine()
    {
        var policyId = Guid.NewGuid();
        var claim1   = Claim.File(policyId, new DateTime(2025, 3, 10), "Rear-end collision.");
        var claim2   = Claim.File(policyId, new DateTime(2024, 11, 5), "Windshield crack.");

        var prompt = RiskPromptBuilder.Build(_customer, _vehicle, [claim1, claim2]);

        Assert.Contains("Prior claims: 2", prompt);
        Assert.Contains("2025-03-10", prompt);
        Assert.Contains("Rear-end collision.", prompt);
        Assert.Contains("2024-11-05", prompt);
        Assert.Contains("Windshield crack.", prompt);
    }

    [Fact]
    public void Build_ContainsDriverAndVehicleDetails()
    {
        var prompt = RiskPromptBuilder.Build(_customer, _vehicle, []);

        Assert.Contains("Alice Smith", prompt);
        Assert.Contains("age 28", prompt);
        Assert.Contains("90001", prompt);
        Assert.Contains("2019 Honda Civic", prompt);
    }

    [Fact]
    public void Build_ContainsFormatInstruction()
    {
        var prompt = RiskPromptBuilder.Build(_customer, _vehicle, []);

        Assert.Contains("LEVEL:", prompt);
        Assert.Contains("Low, Medium, or High", prompt);
    }
}
