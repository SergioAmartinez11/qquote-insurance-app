using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Exceptions;

namespace QQuote.Insurance.Domain.UnitTests.Entities;

public class VehicleTests
{
    [Fact]
    public void Constructor_ValidData_CreatesVehicle()
    {
        var vehicle = new Vehicle(2020, "Toyota", "Camry");

        Assert.Equal(2020, vehicle.Year);
        Assert.Equal("Toyota", vehicle.Make);
        Assert.Equal("Camry", vehicle.Model);
    }

    [Fact]
    public void Constructor_YearBelowNineteenHundred_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new Vehicle(1899, "Toyota", "Camry"));
    }

    [Fact]
    public void Constructor_YearAboveCurrentPlusOne_ThrowsDomainException()
    {
        var tooFuture = DateTime.UtcNow.Year + 2;
        Assert.Throws<DomainException>(() => new Vehicle(tooFuture, "Toyota", "Camry"));
    }

    [Fact]
    public void Constructor_YearCurrentPlusOne_Succeeds()
    {
        var nextYear = DateTime.UtcNow.Year + 1;
        var vehicle = new Vehicle(nextYear, "Toyota", "Camry");
        Assert.Equal(nextYear, vehicle.Year);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyMake_ThrowsDomainException(string make)
    {
        Assert.Throws<DomainException>(() => new Vehicle(2020, make, "Camry"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyModel_ThrowsDomainException(string model)
    {
        Assert.Throws<DomainException>(() => new Vehicle(2020, "Toyota", model));
    }

    [Fact]
    public void Constructor_MakeIsTrimmed()
    {
        var vehicle = new Vehicle(2020, "  Toyota  ", "Camry");
        Assert.Equal("Toyota", vehicle.Make);
    }

    [Fact]
    public void Constructor_ModelIsTrimmed()
    {
        var vehicle = new Vehicle(2020, "Toyota", "  Camry  ");
        Assert.Equal("Camry", vehicle.Model);
    }

    [Fact]
    public void Age_ReturnsCurrentYearMinusVehicleYear()
    {
        var year = DateTime.UtcNow.Year - 5;
        var vehicle = new Vehicle(year, "Toyota", "Camry");
        Assert.Equal(5, vehicle.Age());
    }
}
