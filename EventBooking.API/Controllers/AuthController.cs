using EventBooking.API.Contracts.Auth;
using EventBooking.API.Common;
using EventBooking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        return ToActionResult(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        return ToActionResult(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var result = await authService.RefreshAsync(request);
        return ToActionResult(result);
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RefreshTokenRequest request)
    {
        var result = await authService.RevokeAsync(request);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.Ok(result.Data));
        }

        return result.ErrorCode switch
        {
            ErrorCodes.Conflict => Conflict(ApiResponse<object>.Fail("Request failed.", result.ErrorMessage ?? "Conflict.")),
            ErrorCodes.NotFound => NotFound(ApiResponse<object>.Fail("Request failed.", result.ErrorMessage ?? "Not found.")),
            ErrorCodes.Forbidden => StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail("Request failed.", result.ErrorMessage ?? "Forbidden.")),
            ErrorCodes.BadRequest => BadRequest(ApiResponse<object>.Fail("Request failed.", result.ErrorMessage ?? "Bad request.")),
            _ => Unauthorized(ApiResponse<object>.Fail("Request failed.", result.ErrorMessage ?? "Unauthorized."))
        };
    }
}
