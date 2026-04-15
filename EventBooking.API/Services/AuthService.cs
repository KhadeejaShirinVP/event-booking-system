using System.Security.Cryptography;
using System.Text;
using EventBooking.API.Common;
using EventBooking.API.Contracts.Auth;
using EventBooking.API.Contracts.Common;
using EventBooking.API.Model;
using EventBooking.API.Repositories;

namespace EventBooking.API.Services;

public class AuthService(IAuthRepository authRepository, IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<ServiceResult<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await authRepository.EmailExistsAsync(normalizedEmail);
        if (exists)
        {
            return ServiceResult<RegisterResponse>.Failure("Email is already registered.", ErrorCodes.Conflict);
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role.Trim()
        };

        await authRepository.AddUserAsync(user);
        await authRepository.SaveChangesAsync();

        return ServiceResult<RegisterResponse>.Success(new RegisterResponse
        {
            Message = "Registration successful. Please login to get access token.",
            UserId = user.Id
        });
    }

    public async Task<ServiceResult<TokenResponse>> LoginAsync(LoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await authRepository.GetUserByEmailAsync(normalizedEmail);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return ServiceResult<TokenResponse>.Failure("Invalid email or password.", ErrorCodes.Unauthorized);
        }

        if (!user.IsEmailVerified)
        {
            return ServiceResult<TokenResponse>.Failure("Email is not verified yet.", ErrorCodes.Unauthorized);
        }

        var tokenResponse = await IssueTokensAsync(user);
        return ServiceResult<TokenResponse>.Success(tokenResponse);
    }

    public async Task<ServiceResult<TokenResponse>> RefreshAsync(RefreshTokenRequest request)
    {
        var incomingHash = ComputeSha256(request.RefreshToken);
        var existingToken = await authRepository.GetRefreshTokenWithUserAsync(incomingHash);

        if (existingToken is null || existingToken.User is null)
        {
            return ServiceResult<TokenResponse>.Failure("Invalid refresh token.", ErrorCodes.Unauthorized);
        }

        if (existingToken.RevokedAt.HasValue || existingToken.ExpiresAt <= DateTime.UtcNow)
        {
            return ServiceResult<TokenResponse>.Failure("Refresh token is expired or revoked.", ErrorCodes.Unauthorized);
        }

        existingToken.RevokedAt = DateTime.UtcNow;
        var newTokenResponse = await IssueTokensAsync(existingToken.User);
        existingToken.ReplacedByTokenHash = ComputeSha256(newTokenResponse.RefreshToken);
        await authRepository.SaveChangesAsync();

        return ServiceResult<TokenResponse>.Success(newTokenResponse);
    }

    public async Task<ServiceResult<MessageResponse>> RevokeAsync(RefreshTokenRequest request)
    {
        var tokenHash = ComputeSha256(request.RefreshToken);
        var existingToken = await authRepository.GetRefreshTokenByHashAsync(tokenHash);
        if (existingToken is null)
        {
            return ServiceResult<MessageResponse>.Failure("Refresh token not found.", ErrorCodes.NotFound);
        }

        existingToken.RevokedAt = DateTime.UtcNow;
        await authRepository.SaveChangesAsync();
        return ServiceResult<MessageResponse>.Success(new MessageResponse
        {
            Message = "Refresh token revoked successfully."
        });
    }

    private async Task<TokenResponse> IssueTokensAsync(User user)
    {
        var accessToken = jwtTokenService.GenerateToken(user);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        await authRepository.AddRefreshTokenAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = ComputeSha256(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        });

        await authRepository.SaveChangesAsync();

        return new TokenResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600
        };
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
