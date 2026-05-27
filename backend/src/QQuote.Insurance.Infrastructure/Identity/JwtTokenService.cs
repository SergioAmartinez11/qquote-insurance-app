using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QQuote.Insurance.Application.Common.Interfaces;
using QQuote.Insurance.Domain.Entities;
using SecurityClaim = System.Security.Claims.Claim;
using ClaimTypes   = System.Security.Claims.ClaimTypes;

namespace QQuote.Insurance.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings          _settings;
    private readonly SymmetricSecurityKey _key;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
        _key      = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
    }

    public string GenerateToken(Customer customer)
    {
        var claims = new[]
        {
            new SecurityClaim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new SecurityClaim(ClaimTypes.Email,          customer.Email),
            new SecurityClaim(ClaimTypes.Name,           customer.FullName),
            new SecurityClaim("age",                     customer.Age.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:             _settings.Issuer,
            audience:           _settings.Audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
