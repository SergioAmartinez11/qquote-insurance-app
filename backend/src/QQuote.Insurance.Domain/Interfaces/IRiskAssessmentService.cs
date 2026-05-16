using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Domain.Interfaces;

public interface IRiskAssessmentService
{
    Task<RiskAssessmentResult> AssessAsync(
        Customer          customer,
        Vehicle           vehicle,
        CancellationToken ct = default);
}

public record RiskAssessmentResult(RiskLevel Level, string Explanation);
