using System.Runtime.CompilerServices;
using MongoDB.Driver;
using RailwayApp.Application.Models;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Services;

public class TicketBookingService(IMongoClient mongoClient,
    ICarriageSeatService carriageSeatService,
    IUserAccountRepository userAccountRepository,
    ISeatLockRepository seatLockRepository,
    ICarriageTemplateService carriageTemplateService,
    IPriceCalculationService priceCalculationService,
    IScheduleService scheduleService) : ITicketBookingService
{ 
    public static readonly TimeSpan ReservationTime = TimeSpan.FromMinutes(20);
    private InfoSeatSearchDto MapInfoSeatSearchDto(BookSeatRequest dto, CarriageTemplate carriageTemplate)
    {
        return new InfoSeatSearchDto
        {
            CarriageTemplateId = carriageTemplate.Id,
            ConcreteRouteId = dto.ConcreteRouteId,
            EndSegmentNumber = dto.EndSegmentNumber,
            SeatNumber = dto.SeatNumber,
            StartSegmentNumber = dto.StartSegmentNumber,
        };
    }

    private InfoRouteSegmentSearchPerCarriageDto MapInfoRouteSegmentSearchPerCarriageDto(Guid carriageTemplateId, BookSeatRequest seatRequest)
    {
        return new InfoRouteSegmentSearchPerCarriageDto
        {
            CarriageTemplateId = carriageTemplateId,
            ConcreteRouteId = seatRequest.ConcreteRouteId,
            StartSegmentNumber = seatRequest.StartSegmentNumber,
            EndSegmentNumber = seatRequest.EndSegmentNumber
        };
    }
    public async Task<Guid> BookPlaces(Guid userAccountId, List<BookSeatRequest> request)
    {
        using var session = await mongoClient.StartSessionAsync();
        try
        {
            session.StartTransaction(new TransactionOptions(
                readConcern: ReadConcern.Snapshot,
                writeConcern: WriteConcern.WMajority));
            
            var userAccount = await userAccountRepository.GetByIdAsync(userAccountId, session);
            if (userAccount == null)
                throw new UserServiceUserNotFoundException(userAccountId);

            if (userAccount.Status == UserAccountStatus.Blocked)
                throw new UserServiceUserBlockedException(userAccountId);

            var lockedSeatInfo = new List<LockedSeatInfo>();

            foreach (var seatRequest in request)
            {
                var carriageTemplates =
                    await carriageTemplateService.GetCarriageTemplateForRouteAsync(seatRequest.ConcreteRouteId, session);

                var carriageTemplate =
                    carriageTemplates.FirstOrDefault(x => x.CarriageNumber == seatRequest.CarriageNumber);

                if (carriageTemplate == null)
                {
                    throw new CarriageTemplateNotFoundException(
                        $"carriage template with number {seatRequest.CarriageNumber} not found for route {seatRequest.ConcreteRouteId}");
                }

                if (await carriageSeatService.IsSeatAvailable(MapInfoSeatSearchDto(seatRequest, carriageTemplate), session) ==
                    false)
                {
                    throw new TicketBookingServiceSeatNotAvailableException(
                        $"Seat {seatRequest.SeatNumber} is not available for ConcreteRouteId {seatRequest.ConcreteRouteId}, " +
                        $"StartSegmentNumber {seatRequest.StartSegmentNumber}, EndSegmentNumber {seatRequest.EndSegmentNumber}");
                }

                var price = await priceCalculationService.CalculatePriceForCarriageAsync(
                    MapInfoRouteSegmentSearchPerCarriageDto(carriageTemplate.Id, seatRequest), session);
                var departureDate =
                    await scheduleService.GetDepartureDateForSegment(seatRequest.ConcreteRouteId,
                        seatRequest.StartSegmentNumber, session);
                var arrivalDate =
                    await scheduleService.GetArrivalDateForSegment(seatRequest.ConcreteRouteId,
                        seatRequest.EndSegmentNumber, session);
                lockedSeatInfo.Add(new LockedSeatInfo
                {
                    CarriageTemplateId = carriageTemplate.Id,
                    ConcreteRouteId = seatRequest.ConcreteRouteId,
                    EndSegmentNumber = seatRequest.EndSegmentNumber,
                    SeatNumber = seatRequest.SeatNumber,
                    StartSegmentNumber = seatRequest.StartSegmentNumber,
                    HasBedLinenSet = seatRequest.HasBedLinenSet,
                    PassengerData = seatRequest.PassengerData,

                    Carriage = carriageTemplate.CarriageNumber,
                    DepartureDateUtc = departureDate,
                    ArrivalDateUtc = arrivalDate,
                    Price = price
                });
            }

            var seatLock = new SeatLock
            {
                CreatedAtTimeUtc = DateTime.UtcNow,
                ExpirationTimeUtc = DateTime.UtcNow.Add(ReservationTime),
                Status = SeatLockStatus.Active,
                LockedSeatInfos = lockedSeatInfo,
                UserAccountId = userAccountId
            };
            await seatLockRepository.AddAsync(seatLock, session);
            await session.CommitTransactionAsync();
            return seatLock.Id;
        }
        catch (Exception ex)
        {
            if(session.IsInTransaction)
                await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task<bool> CancelBookPlaces(Guid userAccountId, Guid seatLockId)
    {
        using var session = await mongoClient.StartSessionAsync();
        try
        {
            session.StartTransaction(new TransactionOptions(
                readConcern: ReadConcern.Snapshot,
                writeConcern: WriteConcern.WMajority));


            var userAccount = await userAccountRepository.GetByIdAsync(userAccountId, session);
            if (userAccount == null)
            {
                throw new UserServiceUserNotFoundException(userAccountId);
            }

            if (userAccount.Status == UserAccountStatus.Blocked)
            {
                throw new UserServiceUserBlockedException(userAccountId);
            }


            var seatLock = await seatLockRepository.GetByIdAsync(seatLockId, session);
            if (seatLock == null)
            {
                throw new TicketBookingServiceSeatLockNotFoundException(seatLockId);
            }

            var updateResult = await seatLockRepository.UpdateStatusAsync(seatLockId, SeatLockStatus.Cancelled, session);
            await session.CommitTransactionAsync();
            return updateResult;
        }
        catch (Exception ex)
        {
            if(session.IsInTransaction)
                await session.AbortTransactionAsync();
            throw;
        }
    }
}