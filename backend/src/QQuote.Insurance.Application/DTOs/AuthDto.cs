namespace QQuote.Insurance.Application.DTOs;

public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    int    Age,
    string ZipCode);

public record LoginRequest(string Email, string Password);

public record GoogleSignInRequest(string Credential);

public record AuthResponse(
    string   Token,
    string   FullName,
    string   Email,
    DateTime ExpiresAt);
