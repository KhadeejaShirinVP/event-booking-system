using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventBooking.API.Model;

public class Event
{
    public int Id { get; set; }

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

    [JsonIgnore]
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
