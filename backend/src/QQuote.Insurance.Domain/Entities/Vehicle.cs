using QQuote.Insurance.Domain.Exceptions;

namespace QQuote.Insurance.Domain.Entities;

public sealed class Vehicle
{
    public int    Year  { get; }
    public string Make  { get; }
    public string Model { get; }

    private Vehicle() { }

    public Vehicle(int year, string make, string model)
    {
        if (year < 1900 || year > DateTime.UtcNow.Year + 1)
            throw new DomainException($"Vehicle year {year} is not valid.");
        if (string.IsNullOrWhiteSpace(make))
            throw new DomainException("Vehicle make is required.");
        if (string.IsNullOrWhiteSpace(model))
            throw new DomainException("Vehicle model is required.");

        Year  = year;
        Make  = make.Trim();
        Model = model.Trim();
    }

    public int Age() => DateTime.UtcNow.Year - Year;
}
