using System.ComponentModel.DataAnnotations;

namespace EventBooking.API.Contracts.Bookings;

public class CreateBookingRequest
{
    [Required]
    public int EventId { get; set; }
}
