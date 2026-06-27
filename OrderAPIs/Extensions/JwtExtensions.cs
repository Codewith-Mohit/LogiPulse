using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrderAPIs.Models;

namespace OrderAPIs.Extensions;

public static class JwtExtensions
{
    private static string GetJwtConfigValue(IConfigurationSection section, string key, string envVar, string fallback)
    {
        var value = section[key];
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : Environment.GetEnvironmentVariable(envVar) ?? fallback;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = GetJwtConfigValue(jwtSettings, "Key", "JWT_SECRET", "dev-super-secret-key-change-me-123!");
        var issuer = GetJwtConfigValue(jwtSettings, "Issuer", "JWT_ISSUER", "logipulse");
        var audience = GetJwtConfigValue(jwtSettings, "Audience", "JWT_AUDIENCE", "logipulse-api");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static string GenerateJwtToken(this User user, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = GetJwtConfigValue(jwtSettings, "Key", "JWT_SECRET", "dev-super-secret-key-change-me-123!");
        var issuer = GetJwtConfigValue(jwtSettings, "Issuer", "JWT_ISSUER", "logipulse");
        var audience = GetJwtConfigValue(jwtSettings, "Audience", "JWT_AUDIENCE", "logipulse-api");
        var expiryMinutesString = jwtSettings["ExpiryMinutes"];
        if (string.IsNullOrWhiteSpace(expiryMinutesString))
        {
            expiryMinutesString = Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "60";
        }

        var expiryMinutes = int.Parse(expiryMinutesString);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
