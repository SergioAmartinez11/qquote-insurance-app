using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.Exceptions;

namespace QQuote.Insurance.Domain.UnitTests.Entities;

public class CustomerTests
{
    [Fact]
    public void Create_ValidData_CreatesCustomerWithCorrectProperties()
    {
        var customer = Customer.Create("John Doe", "john@example.com", "hash", 30, "10001");

        Assert.Equal("John Doe", customer.FullName);
        Assert.Equal("john@example.com", customer.Email);
        Assert.Equal("hash", customer.PasswordHash);
        Assert.Equal(30, customer.Age);
        Assert.Equal("10001", customer.ZipCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespaceFullName_ThrowsDomainException(string name)
    {
        Assert.Throws<DomainException>(() =>
            Customer.Create(name, "john@example.com", "hash", 30, "10001"));
    }

    [Theory]
    [InlineData("noemail")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmailWithoutAt_ThrowsDomainException(string email)
    {
        Assert.Throws<DomainException>(() =>
            Customer.Create("John Doe", email, "hash", 30, "10001"));
    }

    [Fact]
    public void Create_AgeBelowSixteen_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Customer.Create("John Doe", "john@example.com", "hash", 15, "10001"));
    }

    [Fact]
    public void Create_AgeAboveOneHundred_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Customer.Create("John Doe", "john@example.com", "hash", 101, "10001"));
    }

    [Fact]
    public void Create_AgeExactlySixteen_Succeeds()
    {
        var customer = Customer.Create("John Doe", "john@example.com", "hash", 16, "10001");
        Assert.Equal(16, customer.Age);
    }

    [Fact]
    public void Create_AgeExactlyOneHundred_Succeeds()
    {
        var customer = Customer.Create("John Doe", "john@example.com", "hash", 100, "10001");
        Assert.Equal(100, customer.Age);
    }

    [Fact]
    public void Create_EmailIsStoredLowercaseAndTrimmed()
    {
        var customer = Customer.Create("John Doe", "  JOHN@EXAMPLE.COM  ", "hash", 30, "10001");
        Assert.Equal("john@example.com", customer.Email);
    }

    [Fact]
    public void Create_FullNameIsTrimmed()
    {
        var customer = Customer.Create("  John Doe  ", "john@example.com", "hash", 30, "10001");
        Assert.Equal("John Doe", customer.FullName);
    }

    [Fact]
    public void Create_IdIsNonEmptyGuid()
    {
        var customer = Customer.Create("John Doe", "john@example.com", "hash", 30, "10001");
        Assert.NotEqual(Guid.Empty, customer.Id);
    }

    [Fact]
    public void Create_PriorClaimsIsZero()
    {
        var customer = Customer.Create("John Doe", "john@example.com", "hash", 30, "10001");
        Assert.Equal(0, customer.PriorClaims);
    }

    [Fact]
    public void Create_CreatedAtIsSet()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var customer = Customer.Create("John Doe", "john@example.com", "hash", 30, "10001");
        Assert.True(customer.CreatedAt >= before);
        Assert.True(customer.CreatedAt <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void CreateFromGoogle_ValidData_CreatesCustomer()
    {
        var customer = Customer.CreateFromGoogle("Jane Doe", "jane@example.com");

        Assert.Equal("Jane Doe", customer.FullName);
        Assert.Equal("jane@example.com", customer.Email);
    }

    [Theory]
    [InlineData("noemail")]
    [InlineData("")]
    public void CreateFromGoogle_InvalidEmail_ThrowsDomainException(string email)
    {
        Assert.Throws<DomainException>(() =>
            Customer.CreateFromGoogle("Jane Doe", email));
    }

    [Fact]
    public void CreateFromGoogle_BlankName_UsesEmailPrefix()
    {
        var customer = Customer.CreateFromGoogle("  ", "janedoe@example.com");
        Assert.Equal("janedoe", customer.FullName);
    }

    [Fact]
    public void CreateFromGoogle_PasswordHashIsEmptyString()
    {
        var customer = Customer.CreateFromGoogle("Jane Doe", "jane@example.com");
        Assert.Equal(string.Empty, customer.PasswordHash);
    }

    [Fact]
    public void CreateFromGoogle_AgeIsZero()
    {
        var customer = Customer.CreateFromGoogle("Jane Doe", "jane@example.com");
        Assert.Equal(0, customer.Age);
    }

    [Fact]
    public void CreateFromGoogle_ZipCodeIsEmptyString()
    {
        var customer = Customer.CreateFromGoogle("Jane Doe", "jane@example.com");
        Assert.Equal(string.Empty, customer.ZipCode);
    }
}
