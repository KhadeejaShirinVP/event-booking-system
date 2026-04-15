using EventBooking.API.Model;

namespace EventBooking.API.Repositories;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<User?> GetUserByEmailAsync(string email);
    Task AddUserAsync(User user);
    Task<RefreshToken?> GetRefreshTokenWithUserAsync(string tokenHash);
    Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash);
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task SaveChangesAsync();
}
