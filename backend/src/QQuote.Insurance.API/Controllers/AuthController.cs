using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using QQuote.Insurance.Application.DTOs;
using QQuote.Insurance.Application.Services;
using QQuote.Insurance.Domain.Exceptions;

namespace QQuote.Insurance.API.Controllers;

[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthAppService _auth;
    private readonly IConfiguration _config;

    public AuthController(AuthAppService auth, IConfiguration config)
    {
        _auth   = auth;
        _config = config;
    }

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

    [HttpPost("google")]
    public async Task<IActionResult> GoogleSignIn(
        [FromBody] GoogleSignInRequest request, CancellationToken ct)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                request.Credential,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_config["GoogleClientId"]]
                });
        }
        catch (InvalidJwtException)
        {
            throw new UnauthorizedException("Invalid Google credential.");
        }

        var result = await _auth.GoogleSignInAsync(payload.Email, payload.Name ?? string.Empty, ct);
        return Ok(result);
    }
}
