using System.ComponentModel.DataAnnotations;

namespace EventBooking.API.Contracts.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
