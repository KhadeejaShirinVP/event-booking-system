using EventBooking.API.Contracts.Common;

namespace EventBooking.API.Contracts.Bookings;

public class BookingDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public DateTime BookingDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public EventSummaryDto? Event { get; set; }
}

public class BookingAdminDto : BookingDto
{
    public UserSummaryDto? User { get; set; }
}
