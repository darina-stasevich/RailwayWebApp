using RailwayApp.Application.Models;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface ITicketBookingService
{
    Task<Guid> BookPlaces(Guid userAccountId, List<BookSeatRequest> request);
    Task<bool> CancelBookPlaces(Guid userAccountId, Guid seatLockId);

    Task<IEnumerable<SeatLockResponse>> GetBooks(Guid userAccountId);
}