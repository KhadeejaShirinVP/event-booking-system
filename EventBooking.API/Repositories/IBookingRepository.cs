using EventBooking.API.Contracts.Bookings;
using EventBooking.API.Model;

namespace EventBooking.API.Repositories;

public interface IBookingRepository
{
    Task<Event?> GetEventByIdAsync(int eventId);
    Task<bool> HasActiveBookingAsync(int userId, int eventId);
    Task AddAsync(Booking booking);
    Task<BookingDto?> GetBookingDtoByIdAsync(int bookingId);
    Task<List<BookingDto>> GetUserBookingsAsync(int userId);
    Task<List<BookingAdminDto>> GetAllBookingsForAdminAsync();
    Task<Booking?> GetByIdAsync(int bookingId);
    Task SaveChangesAsync();
}
