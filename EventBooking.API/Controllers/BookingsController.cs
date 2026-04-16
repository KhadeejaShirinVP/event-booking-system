using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using EventBooking.API.Common;
using EventBooking.API.Contracts.Bookings;
using EventBooking.API.Contracts.Common;
using EventBooking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingRequest request)
    {
        var result = await bookingService.CreateAsync(GetCurrentUserId(), request);
        return ToActionResult(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings()
    {
        var result = await bookingService.GetMyBookingsAsync(GetCurrentUserId());
        return ToActionResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var bookings = await bookingService.GetAllAsync();
        return Ok(ApiResponse<List<BookingAdminDto>>.Ok(bookings));
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateBookingStatusRequest request)
    {
        var result = await bookingService.UpdateStatusAsync(id, request);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelMyBooking(int id)
    {
        var result = await bookingService.CancelBookingAsync(GetCurrentUserId(), User.IsInRole("Admin"), id);
        return ToActionResult(result);
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : null;
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.Ok(result.Data));
        }

        return result.ErrorCode switch
        {
            ErrorCodes.NotFound => NotFound(ApiResponse<object>.Fail("Request failed.", result.ErrorMessage ?? "Not found.")),
            ErrorCodes.Conflict => Conflict(ApiResponse<object>.Fail("Request failed.", result.ErrorMessage ?? "Conflict.")),
            ErrorCodes.Forbidden => StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail("Request failed.", result.ErrorMessage ?? "Forbidden.")),
            ErrorCodes.BadRequest => BadRequest(ApiResponse<object>.Fail("Request failed.", result.ErrorMessage ?? "Bad request.")),
            _ => Unauthorized(ApiResponse<object>.Fail("Request failed.", result.ErrorMessage ?? "Unauthorized."))
        };
    }
}
