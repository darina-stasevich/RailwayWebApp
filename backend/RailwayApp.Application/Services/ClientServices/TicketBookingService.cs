using System.ComponentModel.DataAnnotations;
using MongoDB.Driver;
using RailwayApp.Application.Models;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Services;

public class TicketBookingService(
    IUnitOfWork unitOfWork,
    ICarriageSeatService carriageSeatService,
    ICarriageTemplateService carriageTemplateService,
    IPriceCalculationService priceCalculationService,
    IScheduleService scheduleService) : ITicketBookingService
{
    public static readonly TimeSpan ReservationTime = TimeSpan.FromMinutes(20);

    public async Task<Guid> BookPlaces(Guid userAccountId, List<BookSeatRequest> request)
    {
        ValidateRequests(request);
        
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

            var lockedSeatInfo = new List<LockedSeatInfo>();

            foreach (var seatRequest in request)
            {
                var carriageTemplates =
                    await carriageTemplateService.GetCarriageTemplateForRouteAsync(seatRequest.ConcreteRouteId,
                        unitOfWork.CurrentSession);

                var carriageTemplate =
                    carriageTemplates.FirstOrDefault(x => x.CarriageNumber == seatRequest.CarriageNumber);

                if (carriageTemplate == null)
                    throw new CarriageTemplateNotFoundException(
                        seatRequest.CarriageNumber, seatRequest.ConcreteRouteId);

                if (await carriageSeatService.IsSeatAvailable(MapInfoSeatSearchDto(seatRequest, carriageTemplate),
                        unitOfWork.CurrentSession) ==
                    false)
                    throw new TicketBookingServiceSeatNotAvailableException(
                        seatRequest.SeatNumber, seatRequest.ConcreteRouteId, seatRequest.StartSegmentNumber,
                        seatRequest.EndSegmentNumber);

                var price = await priceCalculationService.CalculatePriceForCarriageAsync(
                    MapInfoRouteSegmentSearchPerCarriageDto(carriageTemplate.Id, seatRequest), unitOfWork.CurrentSession);
                var departureDate =
                    await scheduleService.GetDepartureDateForSegment(seatRequest.ConcreteRouteId,
                        seatRequest.StartSegmentNumber, unitOfWork.CurrentSession);
                var arrivalDate =
                    await scheduleService.GetArrivalDateForSegment(seatRequest.ConcreteRouteId,
                        seatRequest.EndSegmentNumber, unitOfWork.CurrentSession);
                if (departureDate < DateTime.Now)
                    throw new TicketBookingServiceTrainDepartedException(seatRequest.ConcreteRouteId,
                        seatRequest.CarriageNumber,
                        seatRequest.SeatNumber);
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
            await unitOfWork.SeatLocks.AddAsync(seatLock, unitOfWork.CurrentSession);
            await unitOfWork.CommitTransactionAsync();
            return seatLock.Id;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> CancelBookPlaces(Guid userAccountId, Guid seatLockId)
    {
        await unitOfWork.BeginTransactionAsync(new TransactionOptions(
            ReadConcern.Snapshot,
            writeConcern: WriteConcern.WMajority));

        try
        {
            var userAccount = await unitOfWork.UserAccounts.GetByIdAsync(userAccountId, unitOfWork.CurrentSession);
            if (userAccount == null) throw new UserAccountUserNotFoundException(userAccountId);

            if (userAccount.Status == UserAccountStatus.Blocked)
                throw new UserAccountUserBlockedException(userAccountId);


            var seatLock = await unitOfWork.SeatLocks.GetByIdAsync(seatLockId, unitOfWork.CurrentSession);
            if (seatLock == null) throw new TicketBookingServiceSeatLockNotFoundException(seatLockId);

            var updateResult =
                await unitOfWork.SeatLocks.UpdateStatusAsync(seatLockId, SeatLockStatus.Cancelled, unitOfWork.CurrentSession);
            await unitOfWork.CommitTransactionAsync();
            return updateResult;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private async Task<SeatLockResponse> MapSeatLockResponse(SeatLock seatLock)
    {
        return new SeatLockResponse
        {
            SeatLockId = seatLock.Id,
            ExpirationTimeUtc = seatLock.ExpirationTimeUtc,
            LockedSeatInfos = await MapLockedSeatInfoResponses(seatLock.LockedSeatInfos)
        };
    }
    
    private async Task<List<LockedSeatInfoResponse>> MapLockedSeatInfoResponses(
        IEnumerable<LockedSeatInfo> lockedSeatInfos)
    {
        var response = new List<LockedSeatInfoResponse>();
        
        foreach (var seatInfo in lockedSeatInfos)
        {
            var concreteSegments = await 
                unitOfWork.ConcreteRouteSegments.GetConcreteSegmentsByConcreteRouteIdAsync(seatInfo.ConcreteRouteId);
            
            var startSegment = concreteSegments.FirstOrDefault(x => x.SegmentNumber == seatInfo.StartSegmentNumber);
            if (startSegment == null)
                throw new TicketBookingServiceRouteSegmentNotFound(seatInfo.ConcreteRouteId,
                    seatInfo.StartSegmentNumber);
            var endSegment = concreteSegments.FirstOrDefault(x => x.SegmentNumber == seatInfo.EndSegmentNumber);
            if (endSegment == null)
                throw new TicketBookingServiceRouteSegmentNotFound(seatInfo.ConcreteRouteId,
                    seatInfo.EndSegmentNumber);
            
            response.Add(new LockedSeatInfoResponse
            {
                DepartureDate = seatInfo.DepartureDateUtc,
                ArrivalDate = seatInfo.ArrivalDateUtc,
                Carriage = seatInfo.Carriage,
                SeatNumber = seatInfo.SeatNumber,
                ConcreteRouteId = seatInfo.ConcreteRouteId,
                HasBedLinenSet = seatInfo.HasBedLinenSet,
                PassengerData = seatInfo.PassengerData,
                Price = seatInfo.Price,
                FromStationId = startSegment.FromStationId,
                ToStationId = endSegment.ToStationId
            });
        }

        return response;
    }
    
    public async Task<IEnumerable<SeatLockResponse>> GetBooks(Guid userAccountId)
    {
        var userAccount = await unitOfWork.UserAccounts.GetByIdAsync(userAccountId);
        if (userAccount == null)
        {
            throw new UserAccountUserNotFoundException(userAccountId);
        }

        if (userAccount.Status == UserAccountStatus.Blocked)
        {
            throw new UserAccountUserBlockedException(userAccountId);
        }

        var seatLocks = await unitOfWork.SeatLocks.GetByUserAccountIdAsync(userAccountId);
        var activeSeatLocks = seatLocks.Where(s => s.Status == SeatLockStatus.Active);
        var response = new List<SeatLockResponse>();
        foreach (var seatLock in activeSeatLocks)
        {
            response.Add(await MapSeatLockResponse(seatLock));
        }

        return response;
    }

    private InfoSeatSearchDto MapInfoSeatSearchDto(BookSeatRequest dto, CarriageTemplate carriageTemplate)
    {
        return new InfoSeatSearchDto
        {
            CarriageTemplateId = carriageTemplate.Id,
            ConcreteRouteId = dto.ConcreteRouteId,
            EndSegmentNumber = dto.EndSegmentNumber,
            SeatNumber = dto.SeatNumber,
            StartSegmentNumber = dto.StartSegmentNumber
        };
    }

    private InfoRouteSegmentSearchPerCarriageDto MapInfoRouteSegmentSearchPerCarriageDto(Guid carriageTemplateId,
        BookSeatRequest seatRequest)
    {
        return new InfoRouteSegmentSearchPerCarriageDto
        {
            CarriageTemplateId = carriageTemplateId,
            ConcreteRouteId = seatRequest.ConcreteRouteId,
            StartSegmentNumber = seatRequest.StartSegmentNumber,
            EndSegmentNumber = seatRequest.EndSegmentNumber
        };
    }

    private void ValidateRequests(List<BookSeatRequest> requests)
    {
        var today = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        foreach (var request in requests)
            if (today < request.PassengerData.BirthDate)
                throw new ValidationException("validation of birth date failed");
    }
}