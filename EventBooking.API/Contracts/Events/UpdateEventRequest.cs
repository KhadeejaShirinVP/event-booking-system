using System.ComponentModel.DataAnnotations;

namespace EventBooking.API.Contracts.Events;

public class UpdateEventRequest
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(0, 1000000)]
    public decimal Price { get; set; }
}
