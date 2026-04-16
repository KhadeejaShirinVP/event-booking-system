namespace EventBooking.API.Contracts.Auth;

public class RegisterResponse
{
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; }
}
