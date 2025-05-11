using System.Security.Claims;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Web.Middlewares;

public class UserSessionValidatorMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IUserAccountRepository userAccountRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
            {
                if (Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var userAccount = await userAccountRepository.GetByIdAsync(userId);

                    if (userAccount == null)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }

                    if (userAccount.Status != UserAccountStatus.Active)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("User account is inactive.");
                        return;
                    }
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid user identifier format in token.");
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("User identifier not found in token.");
                return;
            }
        }

        await next(context);
    }
}

public static class UserSessionValidatorMiddlewareExtensions
{
    public static IApplicationBuilder UseUserSessionValidator(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserSessionValidatorMiddleware>();
    }
}