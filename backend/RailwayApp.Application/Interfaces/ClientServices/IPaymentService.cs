using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface IPaymentService
{
    Task<List<Ticket>> PayTickets(Guid userAccountId, Guid seatLockId);
    Task<bool> CancelTicket(Guid userAccountId, Guid ticketId);
}