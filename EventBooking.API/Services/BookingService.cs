using EventBooking.API.Common;
using EventBooking.API.Contracts.Bookings;
using EventBooking.API.Contracts.Common;
using EventBooking.API.Model;
using EventBooking.API.Repositories;

namespace EventBooking.API.Services;

public class BookingService(IBookingRepository bookingRepository) : IBookingService
{
    public async Task<ServiceResult<BookingDto>> CreateAsync(int? userId, CreateBookingRequest request)
    {
        if (userId is null)
        {
            return ServiceResult<BookingDto>.Failure("Invalid token claims.", ErrorCodes.Unauthorized);
        }

        var eventItem = await bookingRepository.GetEventByIdAsync(request.EventId);
        if (eventItem is null)
        {
            return ServiceResult<BookingDto>.Failure("Event not found.", ErrorCodes.NotFound);
        }

        if (eventItem.Date <= DateTime.UtcNow)
        {
            return ServiceResult<BookingDto>.Failure("Cannot book an event in the past.", ErrorCodes.BadRequest);
        }

        var alreadyBooked = await bookingRepository.HasActiveBookingAsync(userId.Value, request.EventId);
        if (alreadyBooked)
        {
            return ServiceResult<BookingDto>.Failure("You already have an active booking for this event.", ErrorCodes.Conflict);
        }

        var booking = new Booking
        {
            UserId = userId.Value,
            EventId = request.EventId,
            BookingDate = DateTime.UtcNow,
            Status = "Confirmed"
        };

        await bookingRepository.AddAsync(booking);
        await bookingRepository.SaveChangesAsync();

        var response = await bookingRepository.GetBookingDtoByIdAsync(booking.Id);
        if (response is null)
        {
            return ServiceResult<BookingDto>.Failure("Unable to load booking response.", ErrorCodes.NotFound);
        }

        return ServiceResult<BookingDto>.Success(response);
    }

    public async Task<ServiceResult<List<BookingDto>>> GetMyBookingsAsync(int? userId)
    {
        if (userId is null)
        {
            return ServiceResult<List<BookingDto>>.Failure("Invalid token claims.", ErrorCodes.Unauthorized);
        }

        var bookings = await bookingRepository.GetUserBookingsAsync(userId.Value);
        return ServiceResult<List<BookingDto>>.Success(bookings);
    }

    public Task<List<BookingAdminDto>> GetAllAsync() => bookingRepository.GetAllBookingsForAdminAsync();

    public async Task<ServiceResult<MessageResponse>> UpdateStatusAsync(int id, UpdateBookingStatusRequest request)
    {
        var booking = await bookingRepository.GetByIdAsync(id);
        if (booking is null)
        {
            return ServiceResult<MessageResponse>.Failure("Booking not found.", ErrorCodes.NotFound);
        }

        booking.Status = request.Status.Trim();
        await bookingRepository.SaveChangesAsync();

        return ServiceResult<MessageResponse>.Success(new MessageResponse
        {
            Message = "Booking status updated successfully."
        });
    }

    public async Task<ServiceResult<MessageResponse>> CancelBookingAsync(int? userId, bool isAdmin, int bookingId)
    {
        if (userId is null)
        {
            return ServiceResult<MessageResponse>.Failure("Invalid token claims.", ErrorCodes.Unauthorized);
        }

        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
        {
            return ServiceResult<MessageResponse>.Failure("Booking not found.", ErrorCodes.NotFound);
        }

        if (!isAdmin && booking.UserId != userId.Value)
        {
            return ServiceResult<MessageResponse>.Failure("You cannot cancel another user's booking.", ErrorCodes.Forbidden);
        }

        booking.Status = "Cancelled";
        await bookingRepository.SaveChangesAsync();

        return ServiceResult<MessageResponse>.Success(new MessageResponse
        {
            Message = "Booking cancelled successfully."
        });
    }
}
