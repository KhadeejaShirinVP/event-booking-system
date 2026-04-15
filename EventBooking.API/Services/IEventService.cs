using EventBooking.API.Common;
using EventBooking.API.Contracts.Common;
using EventBooking.API.Contracts.Events;

namespace EventBooking.API.Services;

public interface IEventService
{
    Task<List<EventSummaryDto>> GetAllAsync();
    Task<ServiceResult<EventSummaryDto>> GetByIdAsync(int id);
    Task<ServiceResult<EventSummaryDto>> CreateAsync(CreateEventRequest request);
    Task<ServiceResult<EventSummaryDto>> UpdateAsync(int id, UpdateEventRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
