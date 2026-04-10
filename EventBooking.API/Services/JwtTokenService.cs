using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using EventBooking.API.Model;
using Microsoft.IdentityModel.Tokens;

namespace EventBooking.API.Services;

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public string GenerateToken(User user)
    {
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is not configured.");
        var issuer = configuration["Jwt:Issuer"] ?? "EventBooking.API";
        var audience = configuration["Jwt:Audience"] ?? "EventBooking.Client";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("UserId", user.Id.ToString()),
            new(ClaimTypes.Role, user.Role),
            new("Role", user.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
