using EventBooking.API.Contracts.Auth;
using EventBooking.API.Data;
using EventBooking.API.Model;
using EventBooking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EventBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ApplicationDbContext dbContext, IJwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await dbContext.Users.AnyAsync(u => u.Email == normalizedEmail);
        if (exists)
        {
            return Conflict(new { message = "Email is already registered." });
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role.Trim()
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "Registration successful. Please login to get access token.",
            userId = user.Id
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        if (!user.IsEmailVerified)
        {
            return Unauthorized(new { message = "Email is not verified yet." });
        }

        var (token, refreshToken) = await IssueTokensAsync(user);
        return Ok(new { token, refreshToken, expiresIn = 3600 });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var incomingHash = ComputeSha256(request.RefreshToken);
        var existingToken = await dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == incomingHash);

        if (existingToken is null || existingToken.User is null)
        {
            return Unauthorized(new { message = "Invalid refresh token." });
        }

        if (existingToken.RevokedAt.HasValue || existingToken.ExpiresAt <= DateTime.UtcNow)
        {
            return Unauthorized(new { message = "Refresh token is expired or revoked." });
        }

        existingToken.RevokedAt = DateTime.UtcNow;
        var (token, newRefreshToken) = await IssueTokensAsync(existingToken.User);
        existingToken.ReplacedByTokenHash = ComputeSha256(newRefreshToken);
        await dbContext.SaveChangesAsync();

        return Ok(new { token, refreshToken = newRefreshToken, expiresIn = 3600 });
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RefreshTokenRequest request)
    {
        var tokenHash = ComputeSha256(request.RefreshToken);
        var existingToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        if (existingToken is null)
        {
            return NotFound(new { message = "Refresh token not found." });
        }

        existingToken.RevokedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return Ok(new { message = "Refresh token revoked successfully." });
    }

    private async Task<(string accessToken, string refreshToken)> IssueTokensAsync(User user)
    {
        var accessToken = jwtTokenService.GenerateToken(user);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = ComputeSha256(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
        return (accessToken, refreshToken);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
