using EventBooking.API.Common;
using EventBooking.API.Contracts.Common;
using EventBooking.API.Contracts.Events;
using EventBooking.API.Model;
using EventBooking.API.Repositories;

namespace EventBooking.API.Services;

public class EventService(IEventRepository eventRepository) : IEventService
{
    public Task<List<EventSummaryDto>> GetAllAsync() => eventRepository.GetAllSummariesAsync();

    public async Task<ServiceResult<EventSummaryDto>> GetByIdAsync(int id)
    {
        var eventItem = await eventRepository.GetSummaryByIdAsync(id);
        return eventItem is null
            ? ServiceResult<EventSummaryDto>.Failure("Event not found.", ErrorCodes.NotFound)
            : ServiceResult<EventSummaryDto>.Success(eventItem);
    }

    public async Task<ServiceResult<EventSummaryDto>> CreateAsync(CreateEventRequest request)
    {
        var eventEntity = new Event
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Date = request.Date,
            Location = request.Location.Trim(),
            Price = request.Price
        };

        await eventRepository.AddAsync(eventEntity);
        await eventRepository.SaveChangesAsync();

        return ServiceResult<EventSummaryDto>.Success(new EventSummaryDto
        {
            Id = eventEntity.Id,
            Title = eventEntity.Title,
            Description = eventEntity.Description,
            Date = eventEntity.Date,
            Location = eventEntity.Location,
            Price = eventEntity.Price
        });
    }

    public async Task<ServiceResult<EventSummaryDto>> UpdateAsync(int id, UpdateEventRequest request)
    {
        var eventEntity = await eventRepository.GetByIdAsync(id);
        if (eventEntity is null)
        {
            return ServiceResult<EventSummaryDto>.Failure("Event not found.", ErrorCodes.NotFound);
        }

        eventEntity.Title = request.Title.Trim();
        eventEntity.Description = request.Description.Trim();
        eventEntity.Date = request.Date;
        eventEntity.Location = request.Location.Trim();
        eventEntity.Price = request.Price;

        await eventRepository.SaveChangesAsync();

        return ServiceResult<EventSummaryDto>.Success(new EventSummaryDto
        {
            Id = eventEntity.Id,
            Title = eventEntity.Title,
            Description = eventEntity.Description,
            Date = eventEntity.Date,
            Location = eventEntity.Location,
            Price = eventEntity.Price
        });
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var eventEntity = await eventRepository.GetByIdAsync(id);
        if (eventEntity is null)
        {
            return ServiceResult<bool>.Failure("Event not found.", ErrorCodes.NotFound);
        }

        eventRepository.Remove(eventEntity);
        await eventRepository.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }
}
