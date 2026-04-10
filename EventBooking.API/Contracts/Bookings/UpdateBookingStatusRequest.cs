using System.ComponentModel.DataAnnotations;

namespace EventBooking.API.Contracts.Bookings;

public class UpdateBookingStatusRequest
{
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;
}
