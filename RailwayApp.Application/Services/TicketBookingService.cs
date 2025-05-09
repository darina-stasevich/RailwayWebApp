using System.Runtime.CompilerServices;
using RailwayApp.Application.Models;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Services;

public class TicketBookingService(ICarriageSeatService carriageSeatService,
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
        var userAccount = await userAccountRepository.GetByIdAsync(userAccountId);
        if (userAccount == null)
        {
            throw new UserServiceUserNotFoundException(userAccountId);
        }

        if (userAccount.Status == UserAccountStatus.Blocked)
        {
            throw new UserServiceUserBlockedException(userAccountId);
        }

        var lockedSeatInfo = new List<LockedSeatInfo>();
        
        foreach (var seatRequest in request)
        {
            var carriageTemplates =
                await carriageTemplateService.GetCarriageTemplateForRouteAsync(seatRequest.ConcreteRouteId);

            var carriageTemplate =
                carriageTemplates.FirstOrDefault(x => x.CarriageNumber == seatRequest.CarriageNumber);

            if (carriageTemplate == null)
            {
                throw new CarriageTemplateNotFoundException($"carriage template with number {seatRequest.CarriageNumber} not found for route {seatRequest.ConcreteRouteId}");
            }
            if (await carriageSeatService.IsSeatAvailable(MapInfoSeatSearchDto(seatRequest, carriageTemplate)) == false)
            {
                throw new TicketBookingServiceSeatNotAvailableException(
                    $"Seat {seatRequest.SeatNumber} is not available for ConcreteRouteId {seatRequest.ConcreteRouteId}, " +
                    $"StartSegmentNumber {seatRequest.StartSegmentNumber}, EndSegmentNumber {seatRequest.EndSegmentNumber}");
            }

            var price = await priceCalculationService.CalculatePriceForCarriageAsync(MapInfoRouteSegmentSearchPerCarriageDto(carriageTemplate.Id, seatRequest));
            var departureDate =
                await scheduleService.GetDepartureDateForSegment(seatRequest.ConcreteRouteId,
                    seatRequest.StartSegmentNumber);
            var arrivalDate =
                await scheduleService.GetArrivalDateForSegment(seatRequest.ConcreteRouteId,
                    seatRequest.EndSegmentNumber);
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
        await seatLockRepository.AddAsync(seatLock);
        return seatLock.Id;
    }

    public async Task<bool> CancelBookPlaces(Guid userAccountId, Guid seatLockId)
    {
        var userAccount = await userAccountRepository.GetByIdAsync(userAccountId);
        if (userAccount == null)
        {
            throw new UserServiceUserNotFoundException(userAccountId);
        }

        if (userAccount.Status == UserAccountStatus.Blocked)
        {
            throw new UserServiceUserBlockedException(userAccountId);
        }

        
        var seatLock = await seatLockRepository.GetByIdAsync(seatLockId);
        if (seatLock == null)
        {
            throw new TicketBookingServiceSeatLockNotFoundException(seatLockId);
        }

        var updateResult = await seatLockRepository.UpdateStatusAsync(seatLockId, SeatLockStatus.Cancelled);
        return updateResult;
    }
}