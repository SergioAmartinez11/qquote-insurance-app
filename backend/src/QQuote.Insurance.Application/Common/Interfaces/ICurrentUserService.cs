namespace QQuote.Insurance.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid   CustomerId { get; }
    string Email      { get; }
}
