using MongoDB.Driver;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Services;

public class PaymentService(
    IMongoClient mongoClient,
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
            session.StartTransaction(new TransactionOptions(
                ReadConcern.Snapshot,
                writeConcern: WriteConcern.WMajority));


            var userAccount = await userAccountRepository.GetByIdAsync(userAccountId, session);
            if (userAccount == null)
                throw new UserAccountUserNotFoundException(userAccountId);
            if (userAccount.Status == UserAccountStatus.Blocked)
                throw new UserAccountUserBlockedException(userAccountId);

            var seatLock = await seatLockRepository.GetByIdAsync(seatLockId, session);
            if (seatLock == null)
                throw new SeatLockExpiredException(seatLockId);
            if (seatLock.Status != SeatLockStatus.Active)
                throw new SeatLockNotActiveException(seatLockId);
            if (seatLock.ExpirationTimeUtc < DateTime.UtcNow)
                throw new SeatLockExpiredException(seatLockId);
            if (seatLock.UserAccountId != userAccountId)
                throw new SeatLockNotFoundException(seatLockId, userAccountId);

            var prepared = await seatLockRepository.PrepareForProcessingAsync(
                seatLockId,
                DateTime.UtcNow.AddMinutes(10),
                SeatLockStatus.Processing,
                SeatLockStatus.Active,
                session);

            if (prepared == false)
                throw new PaymentServicePreparingFailedException(seatLockId);

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

            var occupiedSeatsUpdateResult =
                await carriageAvailabilityUpdateService.MarkSeatsAsOccupied(seatLock.LockedSeatInfos, session);
            if (!occupiedSeatsUpdateResult)
                throw new PaymentServiceException("updating seats in carriage availability failed");

            var completed = await seatLockRepository.UpdateStatusAsync(seatLockId, SeatLockStatus.Completed, session);
            if (!completed) throw new PaymentServicePaymentFailedException(seatLockId);

            await session.CommitTransactionAsync();

            return tickets;
        }
        catch (Exception e)
        {
            if (session.IsInTransaction) await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task<bool> CancelTicket(Guid userAccountId, Guid ticketId)
    {
        using var session = await mongoClient.StartSessionAsync();
        try
        {
            //session.StartTransaction();
            session.StartTransaction(new TransactionOptions(
                ReadConcern.Snapshot,
                writeConcern: WriteConcern.WMajority));


            var userAccount = await userAccountRepository.GetByIdAsync(userAccountId, session);
            if (userAccount == null)
                throw new UserAccountUserNotFoundException(userAccountId);
            if (userAccount.Status == UserAccountStatus.Blocked)
                throw new UserAccountUserBlockedException(userAccountId);

            var ticket = await ticketRepository.GetByIdAsync(ticketId, session);
            if (ticket == null)
                throw new TicketNotFoundException(ticketId);
            if (ticket.UserAccountId != userAccountId)
                throw new PaymentServiceTicketNotFoundException(ticketId, userAccountId);
            if (ticket.Status != TicketStatus.Payed)
                throw new PaymentServiceTicketNotPayedException(ticketId);

            await ticketRepository.UpdateStatusAsync(ticketId, TicketStatus.Cancelled, session);

            var occupiedSeatUpdateResult =
                await carriageAvailabilityUpdateService.MarkSeatAsFree(MapFreeTicketDto(ticket), session);
            if (!occupiedSeatUpdateResult)
                throw new PaymentServiceException("updating seat in carriage availability failed");

            await session.CommitTransactionAsync();

            return true;
        }
        catch (Exception e)
        {
            if (session.IsInTransaction) await session.AbortTransactionAsync();
            throw;
        }
    }

    private FreeTicketDto MapFreeTicketDto(Ticket ticket)
    {
        return new FreeTicketDto
        {
            Carriage = ticket.Carriage,
            StartSegmentNumber = ticket.StartSegmentNumber,
            EndSegmentNumber = ticket.EndSegmentNumber,
            ConcreteRouteId = ticket.RouteId,
            SeatNumber = ticket.Seat
        };
    }
}