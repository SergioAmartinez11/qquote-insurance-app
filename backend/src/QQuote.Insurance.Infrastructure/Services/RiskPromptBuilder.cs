using QQuote.Insurance.Domain.Entities;

namespace QQuote.Insurance.Infrastructure.Services;

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
