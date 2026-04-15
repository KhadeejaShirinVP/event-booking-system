using EventBooking.API.Contracts.Common;
using EventBooking.API.Model;

namespace EventBooking.API.Repositories;

public interface IEventRepository
{
    Task<List<EventSummaryDto>> GetAllSummariesAsync();
    Task<EventSummaryDto?> GetSummaryByIdAsync(int id);
    Task<Event?> GetByIdAsync(int id);
    Task AddAsync(Event eventEntity);
    void Remove(Event eventEntity);
    Task SaveChangesAsync();
}
