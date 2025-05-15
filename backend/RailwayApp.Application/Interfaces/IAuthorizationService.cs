using RailwayApp.Application.Models;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface IAuthorizationService
{
    Task<LoginResponse?> AuthorizeAsync(LoginRequest request);
}