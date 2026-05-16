using System.Net.Http.Json;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Interfaces;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Infrastructure.Services;

public class ClaudeRiskAssessmentService : IRiskAssessmentService
{
    private readonly HttpClient _http;

    public ClaudeRiskAssessmentService(HttpClient http) => _http = http;

    public async Task<RiskAssessmentResult> AssessAsync(
        Customer customer, Vehicle vehicle, CancellationToken ct = default)
    {
        var prompt = RiskPromptBuilder.Build(customer, vehicle);

        var body = new
        {
            model      = "claude-sonnet-4-6",
            max_tokens = 300,
            messages   = new[] { new { role = "user", content = prompt } }
        };

        var response = await _http.PostAsJsonAsync(
            "https://api.anthropic.com/v1/messages", body, ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ClaudeResponse>(cancellationToken: ct);
        var text   = result?.Content?.FirstOrDefault()?.Text ?? string.Empty;

        return ParseResponse(text);
    }

    private static RiskAssessmentResult ParseResponse(string text)
    {
        var lower = text.ToLowerInvariant();
        var level = lower.Contains("high")   ? RiskLevel.High   :
                    lower.Contains("medium") ? RiskLevel.Medium  :
                                               RiskLevel.Low;
        return new RiskAssessmentResult(level, text.Trim());
    }

    private record ClaudeResponse(List<ContentBlock>? Content);
    private record ContentBlock(string Text);
}
