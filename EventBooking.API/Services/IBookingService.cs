using EventBooking.API.Common;
using EventBooking.API.Contracts.Bookings;
using EventBooking.API.Contracts.Common;

namespace EventBooking.API.Services;

public interface IBookingService
{
    Task<ServiceResult<BookingDto>> CreateAsync(int? userId, CreateBookingRequest request);
    Task<ServiceResult<List<BookingDto>>> GetMyBookingsAsync(int? userId);
    Task<List<BookingAdminDto>> GetAllAsync();
    Task<ServiceResult<MessageResponse>> UpdateStatusAsync(int id, UpdateBookingStatusRequest request);
    Task<ServiceResult<MessageResponse>> CancelBookingAsync(int? userId, bool isAdmin, int bookingId);
}
