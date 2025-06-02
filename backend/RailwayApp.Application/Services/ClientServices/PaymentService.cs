using MongoDB.Driver;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Services;

public class PaymentService(
    IUnitOfWork unitOfWork,
    ICarriageAvailabilityUpdateService carriageAvailabilityUpdateService) : IPaymentService
{
    public async Task<List<Ticket>> PayTickets(Guid userAccountId, Guid seatLockId)
    {
        await unitOfWork.BeginTransactionAsync(new TransactionOptions(
            ReadConcern.Snapshot,
            writeConcern: WriteConcern.WMajority));
        
        try
        {
            var userAccount = await unitOfWork.UserAccounts.GetByIdAsync(userAccountId, unitOfWork.CurrentSession);
            if (userAccount == null)
                throw new UserAccountUserNotFoundException(userAccountId);
            if (userAccount.Status == UserAccountStatus.Blocked)
                throw new UserAccountUserBlockedException(userAccountId);

            var seatLock = await unitOfWork.SeatLocks.GetByIdAsync(seatLockId, unitOfWork.CurrentSession);
            if (seatLock == null)
                throw new SeatLockExpiredException(seatLockId);
            if (seatLock.Status != SeatLockStatus.Active)
                throw new SeatLockNotActiveException(seatLockId);
            if (seatLock.ExpirationTimeUtc < DateTime.UtcNow)
                throw new SeatLockExpiredException(seatLockId);
            if (seatLock.UserAccountId != userAccountId)
                throw new SeatLockNotFoundException(seatLockId, userAccountId);

            var prepared = await unitOfWork.SeatLocks.PrepareForProcessingAsync(
                seatLockId,
                DateTime.UtcNow.AddMinutes(10),
                SeatLockStatus.Processing,
                SeatLockStatus.Active,
                unitOfWork.CurrentSession);

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

            await unitOfWork.Tickets.AddRange(tickets, unitOfWork.CurrentSession);

            var occupiedSeatsUpdateResult =
                await carriageAvailabilityUpdateService.MarkSeatsAsOccupied(seatLock.LockedSeatInfos, unitOfWork.CurrentSession);
            if (!occupiedSeatsUpdateResult)
                throw new PaymentServiceException("updating seats in carriage availability failed");

            var completed = await unitOfWork.SeatLocks.UpdateStatusAsync(seatLockId, SeatLockStatus.Completed, unitOfWork.CurrentSession);
            if (!completed) throw new PaymentServicePaymentFailedException(seatLockId);

            await unitOfWork.CommitTransactionAsync();

            return tickets;
        }
        catch (Exception e)
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> CancelTicket(Guid userAccountId, Guid ticketId)
    {
        await unitOfWork.BeginTransactionAsync(new TransactionOptions(
            ReadConcern.Snapshot,
            writeConcern: WriteConcern.WMajority));
        
        try
        {
            var userAccount = await unitOfWork.UserAccounts.GetByIdAsync(userAccountId, unitOfWork.CurrentSession);
            if (userAccount == null)
                throw new UserAccountUserNotFoundException(userAccountId);
            if (userAccount.Status == UserAccountStatus.Blocked)
                throw new UserAccountUserBlockedException(userAccountId);

            var ticket = await unitOfWork.Tickets.GetByIdAsync(ticketId, unitOfWork.CurrentSession);
            if (ticket == null)
                throw new TicketNotFoundException(ticketId);
            if (ticket.UserAccountId != userAccountId)
                throw new PaymentServiceTicketNotFoundException(ticketId, userAccountId);
            if (ticket.Status != TicketStatus.Payed)
                throw new PaymentServiceTicketNotPayedException(ticketId);

            await unitOfWork.Tickets.UpdateStatusAsync(ticketId, TicketStatus.Cancelled, unitOfWork.CurrentSession);

            var occupiedSeatUpdateResult =
                await carriageAvailabilityUpdateService.MarkSeatAsFree(MapFreeTicketDto(ticket), unitOfWork.CurrentSession);
            if (!occupiedSeatUpdateResult)
                throw new PaymentServiceException("updating seat in carriage availability failed");

            await unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch (Exception e)
        {
            await unitOfWork.RollbackTransactionAsync();
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