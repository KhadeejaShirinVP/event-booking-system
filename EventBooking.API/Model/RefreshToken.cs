using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventBooking.API.Model;

public class RefreshToken
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(128)]
    public string TokenHash { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    [MaxLength(200)]
    public string? ReplacedByTokenHash { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public User? User { get; set; }
}
