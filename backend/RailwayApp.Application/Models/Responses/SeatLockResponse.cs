namespace RailwayApp.Application.Models;

public class SeatLockResponse
{
    public required Guid SeatLockId { get; set; }
    public List<LockedSeatInfoResponse> LockedSeatInfos { get; set; } = new();
    public DateTime ExpirationTimeUtc { get; set; }
}