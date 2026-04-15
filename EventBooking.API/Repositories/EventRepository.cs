using EventBooking.API.Contracts.Common;
using EventBooking.API.Data;
using EventBooking.API.Model;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.API.Repositories;

public class EventRepository(ApplicationDbContext dbContext) : IEventRepository
{
    public Task<List<EventSummaryDto>> GetAllSummariesAsync() =>
        dbContext.Events
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

    public Task<EventSummaryDto?> GetSummaryByIdAsync(int id) =>
        dbContext.Events
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

    public Task<Event?> GetByIdAsync(int id) => dbContext.Events.FindAsync(id).AsTask();

    public Task AddAsync(Event eventEntity) => dbContext.Events.AddAsync(eventEntity).AsTask();

    public void Remove(Event eventEntity) => dbContext.Events.Remove(eventEntity);

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();
}
