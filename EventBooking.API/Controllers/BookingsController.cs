using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using EventBooking.API.Contracts.Common;
using EventBooking.API.Contracts.Bookings;
using EventBooking.API.Data;
using EventBooking.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "Invalid token claims." });
        }

        var eventItem = await dbContext.Events.FindAsync(request.EventId);
        if (eventItem is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (eventItem.Date <= DateTime.UtcNow)
        {
            return BadRequest(new { message = "Cannot book an event in the past." });
        }

        var alreadyBooked = await dbContext.Bookings
            .AnyAsync(b => b.UserId == userId.Value && b.EventId == request.EventId && b.Status != "Cancelled");
        if (alreadyBooked)
        {
            return Conflict(new { message = "You already have an active booking for this event." });
        }

        var booking = new Booking
        {
            UserId = userId.Value,
            EventId = request.EventId,
            BookingDate = DateTime.UtcNow,
            Status = "Confirmed"
        };

        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        var response = await dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.Id == booking.Id)
            .Select(b => new BookingDto
            {
                Id = b.Id,
                UserId = b.UserId,
                EventId = b.EventId,
                BookingDate = b.BookingDate,
                Status = b.Status,
                Event = b.Event == null
                    ? null
                    : new EventSummaryDto
                    {
                        Id = b.Event.Id,
                        Title = b.Event.Title,
                        Description = b.Event.Description,
                        Date = b.Event.Date,
                        Location = b.Event.Location,
                        Price = b.Event.Price
                    }
            })
            .FirstAsync();

        return Ok(response);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "Invalid token claims." });
        }

        var bookings = await dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId.Value)
            .OrderByDescending(b => b.BookingDate)
            .Select(b => new BookingDto
            {
                Id = b.Id,
                UserId = b.UserId,
                EventId = b.EventId,
                BookingDate = b.BookingDate,
                Status = b.Status,
                Event = b.Event == null
                    ? null
                    : new EventSummaryDto
                    {
                        Id = b.Event.Id,
                        Title = b.Event.Title,
                        Description = b.Event.Description,
                        Date = b.Event.Date,
                        Location = b.Event.Location,
                        Price = b.Event.Price
                    }
            })
            .ToListAsync();

        return Ok(bookings);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var bookings = await dbContext.Bookings
            .AsNoTracking()
            .OrderByDescending(b => b.BookingDate)
            .Select(b => new BookingAdminDto
            {
                Id = b.Id,
                UserId = b.UserId,
                EventId = b.EventId,
                BookingDate = b.BookingDate,
                Status = b.Status,
                User = b.User == null
                    ? null
                    : new UserSummaryDto
                    {
                        Id = b.User.Id,
                        Name = b.User.Name,
                        Email = b.User.Email,
                        Role = b.User.Role,
                        IsEmailVerified = b.User.IsEmailVerified
                    },
                Event = b.Event == null
                    ? null
                    : new EventSummaryDto
                    {
                        Id = b.Event.Id,
                        Title = b.Event.Title,
                        Description = b.Event.Description,
                        Date = b.Event.Date,
                        Location = b.Event.Location,
                        Price = b.Event.Price
                    }
            })
            .ToListAsync();

        return Ok(bookings);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateBookingStatusRequest request)
    {
        var booking = await dbContext.Bookings.FindAsync(id);
        if (booking is null)
        {
            return NotFound(new { message = "Booking not found." });
        }

        booking.Status = request.Status.Trim();
        await dbContext.SaveChangesAsync();
        return Ok(new
        {
            booking.Id,
            booking.UserId,
            booking.EventId,
            booking.BookingDate,
            booking.Status
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelMyBooking(int id)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "Invalid token claims." });
        }

        var booking = await dbContext.Bookings.FindAsync(id);
        if (booking is null)
        {
            return NotFound(new { message = "Booking not found." });
        }

        if (booking.UserId != userId.Value)
        {
            return Forbid();
        }

        booking.Status = "Cancelled";
        await dbContext.SaveChangesAsync();
        return Ok(new { message = "Booking cancelled successfully.", bookingId = booking.Id, booking.Status });
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : null;
    }
}
