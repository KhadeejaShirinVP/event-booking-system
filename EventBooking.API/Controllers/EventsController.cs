using EventBooking.API.Contracts.Common;
using EventBooking.API.Contracts.Events;
using EventBooking.API.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventBooking.API.Services;

namespace EventBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await eventService.GetAllAsync();
        return Ok(ApiResponse<List<EventSummaryDto>>.Ok(events));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await eventService.GetByIdAsync(id);
        return ToActionResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateEventRequest request)
    {
        var result = await eventService.CreateAsync(request);
        if (!result.IsSuccess || result.Data is null)
        {
            return ToActionResult(result);
        }

        return StatusCode(StatusCodes.Status201Created, ApiResponse<EventSummaryDto>.Ok(result.Data, "Event created successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateEventRequest request)
    {
        var result = await eventService.UpdateAsync(id, request);
        return ToActionResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await eventService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        return Ok(ApiResponse<object>.Ok(null, "Event deleted successfully."));
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.Ok(result.Data));
        }

        return result.ErrorCode switch
        {
            ErrorCodes.NotFound => NotFound(ApiResponse<object>.Fail("Request failed.", result.ErrorMessage ?? "Not found.")),
            ErrorCodes.BadRequest => BadRequest(ApiResponse<object>.Fail("Request failed.", result.ErrorMessage ?? "Bad request.")),
            ErrorCodes.Conflict => Conflict(ApiResponse<object>.Fail("Request failed.", result.ErrorMessage ?? "Conflict.")),
            _ => BadRequest(ApiResponse<object>.Fail("Request failed.", result.ErrorMessage ?? "Request failed."))
        };
    }
}
