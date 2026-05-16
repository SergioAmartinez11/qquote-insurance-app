using QQuote.Insurance.Domain.Exceptions;

namespace QQuote.Insurance.Domain.ValueObjects;

public sealed class Money
{
    public decimal Amount   { get; }
    public string  Currency { get; }

    private Money() { }

    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO code.");

        Amount   = Math.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    public static Money Of(decimal amount, string currency) => new(amount, currency);
    public static Money Zero(string currency) => new(0m, currency);

    public Money MultiplyBy(decimal factor)
    {
        if (factor < 0)
            throw new DomainException("Cannot multiply by a negative factor.");
        return new Money(Amount * factor, Currency);
    }

    public static Money operator *(Money a, decimal f) => a.MultiplyBy(f);
    public static Money operator *(decimal f, Money a) => a.MultiplyBy(f);

    public override bool Equals(object? obj) =>
        obj is Money other && Amount == other.Amount && Currency == other.Currency;

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public override string ToString() => $"{Currency} {Amount:F2}";
}
