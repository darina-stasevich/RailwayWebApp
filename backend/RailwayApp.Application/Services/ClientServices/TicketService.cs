using RailwayApp.Application.Models;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Services;

public class TicketService(
    ITicketRepository ticketRepository,
    IUserAccountRepository userAccountRepository,
    IConcreteRouteRepository concreteRouteRepository,
    IConcreteRouteSegmentRepository concreteRouteSegmentRepository) : ITicketService
{
    
    public async Task<IEnumerable<TicketDto>> GetActiveTickets(Guid userAccountId)
    {
        await VerifyUserAccount(userAccountId);
        var tickets = await GetAllTickets(userAccountId);
        return await MapTicketDtos(tickets.Where(x => x.Status == TicketStatus.Payed));
    }

    public async Task<IEnumerable<TicketDto>> GetCancelledTickets(Guid userAccountId)
    {
        await VerifyUserAccount(userAccountId);
        var tickets = await GetAllTickets(userAccountId);
        return await MapTicketDtos(tickets.Where(x => x.Status == TicketStatus.Cancelled));
    }

    public async Task<IEnumerable<TicketDto>> GetExpiredTickets(Guid userAccountId)
    {
        await VerifyUserAccount(userAccountId);
        var tickets = await GetAllTickets(userAccountId);
        return await MapTicketDtos(tickets.Where(x => x.Status == TicketStatus.Expired));
    }

    private async Task<IEnumerable<Ticket>> GetAllTickets(Guid userAccountId)
    {
        return await ticketRepository.GetByUserAccountIdAsync(userAccountId);
    }

    private async Task VerifyUserAccount(Guid userAccountId)
    {
        var userAccount = await userAccountRepository.GetByIdAsync(userAccountId);
        if (userAccount == null) throw new UserAccountUserNotFoundException(userAccountId);
        if (userAccount.Status == UserAccountStatus.Blocked) throw new UserAccountUserBlockedException(userAccountId);
    }

    private async Task<IEnumerable<TicketDto>> MapTicketDtos(IEnumerable<Ticket> tickets)
    {
        var ticketDtos = new List<TicketDto>();
        foreach (var ticket in tickets)
        {
            var route = await concreteRouteRepository.GetByIdAsync(ticket.RouteId);
            if(route == null)
                throw new ConcreteRouteNotFoundException(ticket.RouteId);
            var segments = await concreteRouteSegmentRepository.GetConcreteSegmentsByConcreteRouteIdAsync(
                route.Id);
            if(segments == null)
                throw new ConcreteRouteSegmentsNotFoundException(route.Id);
            var startSegment = segments.FirstOrDefault(s => s.SegmentNumber == ticket.StartSegmentNumber);
            if(startSegment == null)
                throw new ConcreteRouteSegmentNotFoundException(ticket.StartSegmentNumber);
            var endSegment = segments.FirstOrDefault(s => s.SegmentNumber == ticket.EndSegmentNumber);
            if(endSegment == null)
                throw new ConcreteRouteSegmentNotFoundException(ticket.EndSegmentNumber);
            ticketDtos.Add(new TicketDto
            {
                TicketId = ticket.Id,
                FromStationId = startSegment.FromStationId,
                ToStationId = endSegment.ToStationId,
                DepartureDate = ticket.DepartureDate,
                ArrivalDate = ticket.ArrivalDate,
                Price = ticket.Price,
                PassengerData = ticket.PassengerData,
                Carriage = ticket.Carriage,
                Seat = ticket.Seat,
                HasBedLinenSet = ticket.HasBedLinenSet,
                PurchaseTime = ticket.PurchaseTime
            });
        }

        return ticketDtos;
    }
}