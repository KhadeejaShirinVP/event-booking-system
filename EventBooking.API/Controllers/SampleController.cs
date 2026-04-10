using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SampleController : ControllerBase
{
    [Authorize]
    [HttpGet("profile")]
    public IActionResult Profile()
    {
        return Ok(new
        {
            message = "Authenticated endpoint reached.",
            userId = User.FindFirst("UserId")?.Value,
            role = User.FindFirst("Role")?.Value
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly()
    {
        return Ok(new { message = "Admin-only endpoint reached." });
    }
}
