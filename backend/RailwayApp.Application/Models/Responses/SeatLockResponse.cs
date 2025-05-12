using RailwayApp.Domain.Entities;

namespace RailwayApp.Application.Models;

public class SeatLockResponse
{
    public List<LockedSeatInfoResponse> LockedSeatInfos { get; set; } = new List<LockedSeatInfoResponse>();

    public DateTime ExpirationTimeUtc { get; set; }

}