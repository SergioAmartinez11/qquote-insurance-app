using QQuote.Insurance.Domain.Exceptions;

namespace QQuote.Insurance.Domain.ValueObjects;

public sealed class RiskLevel
{
    public static readonly RiskLevel Low    = new(nameof(Low),    1.0m);
    public static readonly RiskLevel Medium = new(nameof(Medium), 1.18m);
    public static readonly RiskLevel High   = new(nameof(High),   1.40m);

    public string  Name              { get; }
    public decimal PremiumMultiplier { get; }

    private static readonly IReadOnlyList<RiskLevel> _all = [Low, Medium, High];

    private RiskLevel(string name, decimal multiplier)
    {
        Name              = name;
        PremiumMultiplier = multiplier;
    }

    public static RiskLevel From(string name)
    {
        var level = _all.FirstOrDefault(r =>
            r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (level is null)
            throw new DomainException(
                $"'{name}' is not a valid risk level. Valid: {string.Join(", ", _all.Select(r => r.Name))}");

        return level;
    }

    public static IReadOnlyList<RiskLevel> All() => _all;

    public override bool Equals(object? obj) => obj is RiskLevel other && Name == other.Name;
    public override int GetHashCode() => Name.GetHashCode();
    public static bool operator ==(RiskLevel? a, RiskLevel? b) => a?.Equals(b) ?? b is null;
    public static bool operator !=(RiskLevel? a, RiskLevel? b) => !(a == b);
    public override string ToString() => Name;
}
