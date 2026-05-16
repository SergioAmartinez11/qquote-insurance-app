using QQuote.Insurance.Domain.Entities;

namespace QQuote.Insurance.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(Customer customer);
}
