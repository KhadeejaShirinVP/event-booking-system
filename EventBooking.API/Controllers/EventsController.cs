using EventBooking.API.Contracts.Common;
using EventBooking.API.Contracts.Events;
using EventBooking.API.Data;
using EventBooking.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await dbContext.Events
            .AsNoTracking()
            .OrderBy(e => e.Date)
            .Select(e => new EventSummaryDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Date = e.Date,
                Location = e.Location,
                Price = e.Price
            })
            .ToListAsync();
        return Ok(events);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var eventItem = await dbContext.Events
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EventSummaryDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Date = e.Date,
                Location = e.Location,
                Price = e.Price
            })
            .FirstOrDefaultAsync();
        if (eventItem is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        return Ok(eventItem);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateEventRequest request)
    {
        var eventItem = new Event
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Date = request.Date,
            Location = request.Location.Trim(),
            Price = request.Price
        };

        dbContext.Events.Add(eventItem);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = eventItem.Id }, new EventSummaryDto
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            Description = eventItem.Description,
            Date = eventItem.Date,
            Location = eventItem.Location,
            Price = eventItem.Price
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateEventRequest request)
    {
        var eventItem = await dbContext.Events.FindAsync(id);
        if (eventItem is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        eventItem.Title = request.Title.Trim();
        eventItem.Description = request.Description.Trim();
        eventItem.Date = request.Date;
        eventItem.Location = request.Location.Trim();
        eventItem.Price = request.Price;

        await dbContext.SaveChangesAsync();
        return Ok(new EventSummaryDto
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            Description = eventItem.Description,
            Date = eventItem.Date,
            Location = eventItem.Location,
            Price = eventItem.Price
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eventItem = await dbContext.Events.FindAsync(id);
        if (eventItem is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        dbContext.Events.Remove(eventItem);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
