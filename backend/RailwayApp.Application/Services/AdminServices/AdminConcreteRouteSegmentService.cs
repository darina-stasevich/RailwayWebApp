using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Application.Services.AdminServices;

public class AdminConcreteRouteSegmentService(
    IConcreteRouteSegmentRepository concreteRouteSegmentRepository,
    IAbstractRouteSegmentRepository abstractRouteSegmentRepository,
    IConcreteRouteRepository concreteRouteRepository,
    IStationRepository stationRepository) : IAdminService<ConcreteRouteSegment, Guid>, IAdminConcreteRouteSegmentService
{
    public async Task<IEnumerable<ConcreteRouteSegment>> GetAllItems()
    {
        return await concreteRouteSegmentRepository.GetAllAsync();
    }

    private async Task ValidateData(ConcreteRouteSegment item)
    {
        var abstractRouteSegment = await abstractRouteSegmentRepository.GetByIdAsync(item.AbstractSegmentId);
        if (abstractRouteSegment == null)
            throw new AdminResourceNotFoundException("abstract route segment", item.AbstractSegmentId);
        var concreteRoute = await concreteRouteRepository.GetByIdAsync(item.ConcreteRouteId);
        if (concreteRoute == null)
            throw new AdminResourceNotFoundException("concrete route segment", item.ConcreteRouteId);
        var fromStation = await stationRepository.GetByIdAsync(item.FromStationId);
        if (fromStation == null)
            throw new AdminResourceNotFoundException("from station", item.FromStationId);
        var toStation = await stationRepository.GetByIdAsync(item.ToStationId);
        if (toStation == null)
            throw new AdminResourceNotFoundException("to station", item.ToStationId);
        var concreteRouteSegments = await 
            concreteRouteSegmentRepository.GetConcreteSegmentsByConcreteRouteIdAsync(item.ConcreteRouteId);
        if (concreteRouteSegments.FirstOrDefault(s => s.SegmentNumber == item.SegmentNumber) != default)
            throw new AdminDataConflictException($"concrete route segment {nameof(ConcreteRouteSegment)} with SegmentNumber '{item.SegmentNumber}' already exists for ConcreteRoute ID '{item.ConcreteRouteId}'.");
        
    }
    public async Task<Guid> CreateItem(ConcreteRouteSegment item)
    {
        await ValidateData(item);
        return await concreteRouteSegmentRepository.AddAsync(item);
    }

    public async Task<bool> UpdateItem(Guid id, ConcreteRouteSegment itemToUpdate)
    {
        if (id != itemToUpdate.Id)
        {
            itemToUpdate.Id = id;
        }

        await ValidateData(itemToUpdate);
        var existingSegment = await concreteRouteSegmentRepository.GetByIdAsync(id);
        if (existingSegment == null)
        {
            throw new AdminResourceNotFoundException(nameof(ConcreteRouteSegment), id);
        }
        
        if (existingSegment.SegmentNumber != itemToUpdate.SegmentNumber || 
            existingSegment.ConcreteRouteId != itemToUpdate.ConcreteRouteId) 
        {
            var concreteRouteSegments = await 
                concreteRouteSegmentRepository.GetConcreteSegmentsByConcreteRouteIdAsync(itemToUpdate.ConcreteRouteId);
            if (concreteRouteSegments.Any(s => s.Id != id && s.SegmentNumber == itemToUpdate.SegmentNumber))
            {
                throw new AdminDataConflictException(
                    $"A {nameof(ConcreteRouteSegment)} with SegmentNumber '{itemToUpdate.SegmentNumber}' already exists for ConcreteRoute ID '{itemToUpdate.ConcreteRouteId}'.");
            }
        }
        
        if (itemToUpdate.ConcreteDepartureDate >= itemToUpdate.ConcreteArrivalDate)
        {
            throw new AdminValidationException("Departure date must be earlier than arrival date.");
        }

        bool success = await concreteRouteSegmentRepository.UpdateAsync(itemToUpdate);

        return success;
    }

    public async Task<Guid> DeleteItem(Guid id)
    {
        var existingSegment = await concreteRouteSegmentRepository.GetByIdAsync(id);
        if (existingSegment == null)
        {
            throw new AdminResourceNotFoundException(nameof(ConcreteRouteSegment), id);
        }
        await concreteRouteSegmentRepository.DeleteAsync(id);
        return id;
    }
}