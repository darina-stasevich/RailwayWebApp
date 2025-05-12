using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class AuthorizationService(IConfiguration configuration,
    IUserAccountRepository userAccountRepository,
    IPasswordHasher passwordHasher) : IAuthorizationService
{
    public async Task<LoginResponse?> AuthorizeAsync(LoginRequest request)
    {
        var userAccount = await userAccountRepository.GetByEmailAsync(request.Email);
        if (userAccount == null)
            throw new HttpRequestException("User not found", null, HttpStatusCode.NotFound);
        
        if (!passwordHasher.VerifyHashedPassword(userAccount.HashedPassword, request.Password))
        {
            throw new HttpRequestException("Invalid password", null, HttpStatusCode.Unauthorized);
        }
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userAccount.Id.ToString()),
            new(ClaimTypes.Name, userAccount.Email),
            new(ClaimTypes.Role, userAccount.Role.ToString())
        };
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        var loginResponse = new LoginResponse
        {
            ExpireAt = token.ValidTo,
            Role = userAccount.Role.ToString(),
            Token = tokenString,
            UserName = userAccount.Email
        };
        return loginResponse;
    }
}