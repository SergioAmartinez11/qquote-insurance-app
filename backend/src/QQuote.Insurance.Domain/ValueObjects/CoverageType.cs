using QQuote.Insurance.Domain.Exceptions;

namespace QQuote.Insurance.Domain.ValueObjects;

public sealed class CoverageType
{
    public static readonly CoverageType Basic         = new(nameof(Basic),         50m);
    public static readonly CoverageType Comprehensive = new(nameof(Comprehensive), 120m);
    public static readonly CoverageType Limited       = new(nameof(Limited),       30m);

    public string  Name     { get; }
    public decimal BaseRate { get; }

    private static readonly IReadOnlyList<CoverageType> _all = [Basic, Comprehensive, Limited];

    private CoverageType(string name, decimal baseRate)
    {
        Name     = name;
        BaseRate = baseRate;
    }

    public static CoverageType From(string name)
    {
        var type = _all.FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (type is null)
            throw new DomainException(
                $"'{name}' is not a valid coverage type. Valid: {string.Join(", ", _all.Select(c => c.Name))}");

        return type;
    }

    public static IReadOnlyList<CoverageType> All() => _all;

    public override bool Equals(object? obj) => obj is CoverageType other && Name == other.Name;
    public override int GetHashCode() => Name.GetHashCode();
    public static bool operator ==(CoverageType? a, CoverageType? b) => a?.Equals(b) ?? b is null;
    public static bool operator !=(CoverageType? a, CoverageType? b) => !(a == b);
    public override string ToString() => Name;
}
