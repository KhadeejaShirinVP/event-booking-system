using EventBooking.API.Data;
using EventBooking.API.Model;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.API.Repositories;

public class AuthRepository(ApplicationDbContext dbContext) : IAuthRepository
{
    public Task<bool> EmailExistsAsync(string email) =>
        dbContext.Users.AnyAsync(u => u.Email == email);

    public Task<User?> GetUserByEmailAsync(string email) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task AddUserAsync(User user) =>
        dbContext.Users.AddAsync(user).AsTask();

    public Task<RefreshToken?> GetRefreshTokenWithUserAsync(string tokenHash) =>
        dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

    public Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash) =>
        dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

    public Task AddRefreshTokenAsync(RefreshToken refreshToken) =>
        dbContext.RefreshTokens.AddAsync(refreshToken).AsTask();

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();
}
