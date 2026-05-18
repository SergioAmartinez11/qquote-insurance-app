using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QQuote.Insurance.API.Middleware;
using QQuote.Insurance.Infrastructure.Common;
using QQuote.Insurance.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Render supplies ConnectionStrings__DefaultConnection in postgresql://user:pass@host/db form.
// Npgsql requires key=value format, so we convert it here before AddInfrastructure reads it.
var rawConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
if (rawConn.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
    rawConn.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    var uri  = new Uri(rawConn);
    var info = uri.UserInfo.Split(':', 2);
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var db   = uri.AbsolutePath.TrimStart('/');
    var user = Uri.UnescapeDataString(info[0]);
    var pass = info.Length > 1 ? Uri.UnescapeDataString(info[1]) : "";
    builder.Configuration["ConnectionStrings:DefaultConnection"] =
        $"Host={host};Port={port};Database={db};Username={user};Password={pass};SslMode=Require";
}

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings["Issuer"],
            ValidAudience            = jwtSettings["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.SetIsOriginAllowed(origin =>
        {
            var host = new Uri(origin).Host;
            return host == "localhost" || host.EndsWith(".vercel.app");
        })
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("Frontend");
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
await db.Database.MigrateAsync();

app.Run();
