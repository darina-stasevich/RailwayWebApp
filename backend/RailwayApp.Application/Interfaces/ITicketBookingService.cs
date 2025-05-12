using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface ITicketBookingService
{
    Task<Guid> BookPlaces(Guid userAccountId, List<BookSeatRequest> request);
    Task<bool> CancelBookPlaces(Guid userAccountId, Guid seatLockId);

    Task<IEnumerable<SeatLockResponse>> GetBooks(Guid userAccountId);
}