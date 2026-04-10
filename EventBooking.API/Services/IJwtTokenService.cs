using EventBooking.API.Model;

namespace EventBooking.API.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
}
