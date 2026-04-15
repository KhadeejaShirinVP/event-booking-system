using EventBooking.API.Common;
using EventBooking.API.Contracts.Auth;
using EventBooking.API.Contracts.Common;

namespace EventBooking.API.Services;

public interface IAuthService
{
    Task<ServiceResult<RegisterResponse>> RegisterAsync(RegisterRequest request);
    Task<ServiceResult<TokenResponse>> LoginAsync(LoginRequest request);
    Task<ServiceResult<TokenResponse>> RefreshAsync(RefreshTokenRequest request);
    Task<ServiceResult<MessageResponse>> RevokeAsync(RefreshTokenRequest request);
}
