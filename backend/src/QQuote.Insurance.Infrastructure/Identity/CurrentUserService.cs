using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using QQuote.Insurance.Application.Common.Interfaces;

namespace QQuote.Insurance.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    public Guid   CustomerId { get; }
    public string Email      { get; }

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User;
        CustomerId = Guid.Parse(user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
        Email      = user?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    }
}
