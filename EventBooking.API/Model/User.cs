using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventBooking.API.Model;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "User";

    public bool IsEmailVerified { get; set; } = false;

    [JsonIgnore]
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    [JsonIgnore]
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
