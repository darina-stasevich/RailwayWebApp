namespace RailwayApp.Application.Models;

public class SeatLockResponse
{
    public List<LockedSeatInfoResponse> LockedSeatInfos { get; set; } = new();

    public DateTime ExpirationTimeUtc { get; set; }
}