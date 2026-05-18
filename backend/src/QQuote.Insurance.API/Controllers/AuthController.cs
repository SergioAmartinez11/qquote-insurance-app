using Microsoft.AspNetCore.Mvc;
using QQuote.Insurance.Application.DTOs;
using QQuote.Insurance.Application.Services;

namespace QQuote.Insurance.API.Controllers;


[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthAppService _auth;
    public AuthController(AuthAppService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _auth.RegisterAsync(request, ct);
        return Created("/api/auth/me", result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(request, ct);
        return Ok(result);
    }
}
