using MongoDB.Driver;
using RailwayApp.Application.Models;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Services;

public class PaymentService(IMongoClient mongoClient,
    ISeatLockRepository seatLockRepository,
    IUserAccountRepository userAccountRepository,
    ITicketRepository ticketRepository,
    ICarriageAvailabilityUpdateService carriageAvailabilityUpdateService) : IPaymentService
{
    
    public async Task<List<Ticket>> PayTickets(Guid userAccountId, Guid seatLockId)
    {
        using var session = await mongoClient.StartSessionAsync();
        try
        {
            //session.StartTransaction();
            session.StartTransaction(new TransactionOptions(
                readConcern: ReadConcern.Snapshot,
                writeConcern: WriteConcern.WMajority));
            
            
            var userAccount = await userAccountRepository.GetByIdAsync(userAccountId, session);
            if (userAccount == null)
                throw new UserServiceUserNotFoundException(userAccountId);
            if (userAccount.Status == UserAccountStatus.Blocked)
                throw new UserServiceUserBlockedException(userAccountId);

            var seatLock = await seatLockRepository.GetByIdAsync(seatLockId, session);
            if (seatLock == null)
                throw new SeatLockExpiredException(seatLockId);
            if (seatLock.Status != SeatLockStatus.Active)
                throw new SeatLockNotActiveException(seatLockId);
            if (seatLock.ExpirationTimeUtc < DateTime.UtcNow)
                throw new SeatLockExpiredException(seatLockId);
            if (seatLock.UserAccountId != userAccountId)
                throw new PaymentServiceException($"seatLock {seatLockId} not found for user {userAccountId}");
            
            bool prepared = await seatLockRepository.PrepareForProcessingAsync(
                seatLockId,
                DateTime.UtcNow.AddMinutes(10),
                SeatLockStatus.Processing,
                SeatLockStatus.Active,
                session);

            if (prepared == false)
                throw new PaymentServiceException($"failed to prepare seatLock {seatLockId} for payment");

            var tickets = new List<Ticket>();
            foreach (var seatInfo in seatLock.LockedSeatInfos)
            {
                var ticket = new Ticket
                {
                    RouteId = seatInfo.ConcreteRouteId,
                    UserAccountId = userAccountId,
                    StartSegmentNumber = seatInfo.StartSegmentNumber,
                    EndSegmentNumber = seatInfo.EndSegmentNumber,
                    DepartureDate = seatInfo.DepartureDateUtc,
                    ArrivalDate = seatInfo.ArrivalDateUtc,
                    Price = seatInfo.Price,
                    PassengerData = seatInfo.PassengerData,
                    Carriage = seatInfo.Carriage,
                    Seat = seatInfo.SeatNumber,
                    HasBedLinenSet = seatInfo.HasBedLinenSet,
                    PurchaseTime = DateTime.Now,
                    Status = TicketStatus.Payed
                };
                tickets.Add(ticket);
            }

            await ticketRepository.AddRange(tickets, session);

            var occupiedSeatsUpdateResult = await carriageAvailabilityUpdateService.MarkSeatsAsOccupied(seatLock.LockedSeatInfos, session);
            if (!occupiedSeatsUpdateResult)
                throw new PaymentServiceException("updating seats in carriage availability failed");
            
            bool completed = await seatLockRepository.UpdateStatusAsync(seatLockId, SeatLockStatus.Completed, session);
            if (!completed)
            {
                throw new PaymentServicePaymentFailedException(seatLockId);
            }
            
            await session.CommitTransactionAsync();

            return tickets;
        }
        catch (Exception e)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task<bool> CancelTicket(Guid userAccountId, Guid ticketId)
    {
        throw new NotImplementedException();
    }
}