using System.Text;
using QQuote.Insurance.Domain.Entities;

namespace QQuote.Insurance.Infrastructure.Services;

public static class RiskPromptBuilder
{
    public static string Build(Customer customer, Vehicle vehicle, IReadOnlyList<Claim> claimsHistory)
    {
        var claimsSection = BuildClaimsSection(claimsHistory);

        return $"""
        You are an insurance actuary. Evaluate this driver profile and respond with:
        1. Risk level: exactly one word — Low, Medium, or High
        2. One sentence explanation for the customer

        Driver: {customer.FullName}, age {customer.Age}, zip {customer.ZipCode}
        Vehicle: {vehicle.Year} {vehicle.Make} {vehicle.Model} (age: {vehicle.Age()} years)
        Prior claims: {claimsHistory.Count}
        {claimsSection}
        Format: "LEVEL: [level]. [explanation sentence]"
        """;
    }

    private static string BuildClaimsSection(IReadOnlyList<Claim> claims)
    {
        if (claims.Count == 0)
            return "Claim history: none";

        var sb = new StringBuilder("Claim history:\n");
        foreach (var c in claims)
            sb.AppendLine($"  - {c.IncidentDate:yyyy-MM-dd} | {c.Status} | {c.Description}");

        return sb.ToString().TrimEnd();
    }
}
