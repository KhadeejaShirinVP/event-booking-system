using System.ComponentModel.DataAnnotations;

namespace EventBooking.API.Model;

public class Booking
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int EventId { get; set; }

    [Required]
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;

    public User? User { get; set; }
    public Event? Event { get; set; }
}
