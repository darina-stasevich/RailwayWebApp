namespace RailwayApp.Application.Models;

public class LoginResponse
{
    public string Token { get; set; }
    public DateTime ExpireAt { get; set; }
    public string UserName { get; set; }
    public string Role { get; set; }
}