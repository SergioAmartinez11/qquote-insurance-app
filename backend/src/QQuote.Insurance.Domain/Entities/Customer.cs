using QQuote.Insurance.Domain.Exceptions;

namespace QQuote.Insurance.Domain.Entities;

public class Customer
{
    public Guid     Id           { get; private set; }
    public string   FullName     { get; private set; } = string.Empty;
    public string   Email        { get; private set; } = string.Empty;
    public string   PasswordHash { get; private set; } = string.Empty;
    public int      Age          { get; private set; }
    public string   ZipCode      { get; private set; } = string.Empty;
    public int      PriorClaims  { get; private set; }
    public DateTime CreatedAt    { get; private set; }

    private Customer() { }

    public static Customer Create(
        string fullName,
        string email,
        string passwordHash,
        int    age,
        string zipCode)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("A valid email is required.");
        if (age < 16 || age > 100)
            throw new DomainException("Age must be between 16 and 100.");

        return new Customer
        {
            Id           = Guid.NewGuid(),
            FullName     = fullName.Trim(),
            Email        = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            Age          = age,
            ZipCode      = zipCode.Trim(),
            PriorClaims  = 0,
            CreatedAt    = DateTime.UtcNow
        };
    }

    public static Customer CreateFromGoogle(string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("A valid email is required.");

        return new Customer
        {
            Id           = Guid.NewGuid(),
            FullName     = string.IsNullOrWhiteSpace(fullName) ? email.Split('@')[0] : fullName.Trim(),
            Email        = email.ToLowerInvariant().Trim(),
            PasswordHash = string.Empty,
            Age          = 0,
            ZipCode      = string.Empty,
            PriorClaims  = 0,
            CreatedAt    = DateTime.UtcNow
        };
    }
}
