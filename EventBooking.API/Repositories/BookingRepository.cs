using EventBooking.API.Contracts.Bookings;
using EventBooking.API.Contracts.Common;
using EventBooking.API.Data;
using EventBooking.API.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EventBooking.API.Repositories;

public class BookingRepository(ApplicationDbContext dbContext) : IBookingRepository
{
    public Task<Event?> GetEventByIdAsync(int eventId) => dbContext.Events.FindAsync(eventId).AsTask();

    public Task<bool> HasActiveBookingAsync(int userId, int eventId) =>
        dbContext.Bookings.AnyAsync(b => b.UserId == userId && b.EventId == eventId && b.Status != "Cancelled");

    public Task AddAsync(Booking booking) => dbContext.Bookings.AddAsync(booking).AsTask();

    public Task<BookingDto?> GetBookingDtoByIdAsync(int bookingId) =>
        dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(MapBookingDto())
            .FirstOrDefaultAsync();

    public Task<List<BookingDto>> GetUserBookingsAsync(int userId) =>
        dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .Select(MapBookingDto())
            .ToListAsync();

    public Task<List<BookingAdminDto>> GetAllBookingsForAdminAsync() =>
        dbContext.Bookings
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

    public Task<Booking?> GetByIdAsync(int bookingId) => dbContext.Bookings.FindAsync(bookingId).AsTask();

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();

    private static Expression<Func<Booking, BookingDto>> MapBookingDto()
    {
        return b => new BookingDto
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
        };
    }
}
