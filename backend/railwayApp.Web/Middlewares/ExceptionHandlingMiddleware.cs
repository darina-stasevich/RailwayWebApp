using System.ComponentModel.DataAnnotations; // Для ValidationException
using System.Net;
using System.Text.Json; // Для JsonSerializer и JsonSerializerOptions
using Microsoft.AspNetCore.Mvc; // Для ProblemDetails
using railway_service; // Предполагается, что здесь твои кастомные исключения
using RailwayApp.Domain; // Предполагается, что здесь твои доменные исключения

// Предполагается, что IMyCustomLogger и его реализация (MyCustomFileJsonLogger) определены
// и зарегистрированы в DI.
// Например, в Program.cs:
// builder.Services.AddSingleton<IMyCustomLogger, MyCustomFileJsonLogger>();

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IMyCustomLogger customLogger,
    IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        const string loggerNameForCustomLog = "GlobalExceptionHandler"; // Имя для кастомного логгера

        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path,
            Detail = env.IsDevelopment() ? exception.ToString() : null
        };
        problemDetails.Extensions.Add("requestId", context.TraceIdentifier);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        problemDetails.Status = StatusCodes.Status500InternalServerError;
        problemDetails.Title = "Internal server error"; // Default title

        switch (exception)
        {
            case ValidationException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Ошибка валидации данных";
                logger.LogWarning(ex, "ValidationException : {message}", ex.Message);
                customLogger.Warn("ValidationException occurred", loggerNameForCustomLog, ex,
                    new Dictionary<string, object> { { "Details", ex.Message }, { "ValidationResult", ex.ValidationResult?.ToString() ?? "N/A" } });
                break;
            case UserAccountEmailAlreadyExistsException ex:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = $"Пользователь с email '{ex.Email}' уже существует";
                logger.LogWarning(ex, "UserAccountEmailAlreadyExistsException: Email: {Email}", ex.Email);
                customLogger.Warn("UserAccountEmailAlreadyExistsException occurred", loggerNameForCustomLog, ex,
                    new Dictionary<string, object> { { "Email", ex.Email } });
                break;
            case UserAccountUserNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Пользователь не найден";
                logger.LogWarning(ex, "UserAccountUserNotFoundException: User ID: {UserId}", ex.UserId);
                customLogger.Warn("UserAccountUserNotFoundException occurred", loggerNameForCustomLog, ex,
                    new Dictionary<string, object> { { "UserId", ex.UserId } });
                break;
            case UserAccountUserBlockedException ex:
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                problemDetails.Status = StatusCodes.Status403Forbidden;
                problemDetails.Title = "Учетная запись заблокирована";
                logger.LogWarning(ex, "UserAccountUserBlockedException: User ID: {UserId}", ex.UserId);
                customLogger.Warn("UserAccountUserBlockedException occurred", loggerNameForCustomLog, ex,
                    new Dictionary<string, object> { { "UserId", ex.UserId } });
                break;
            case UserAccountInvalidPasswordException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Неверный email или пароль";
                logger.LogWarning(ex, "UserAccountInvalidPasswordException: Email: {Email}", ex.Email);
                customLogger.Warn("UserAccountInvalidPasswordException occurred", loggerNameForCustomLog, ex,
                    new Dictionary<string, object> { { "Email", ex.Email } });
                break;
            case UserAccountInvalidAgeException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Некорректный возраст";
                logger.LogWarning(ex, "UserAccountInvalidAgeException: BirthDate: {BirthDate}", ex.Date);
                customLogger.Warn("UserAccountInvalidAgeException occurred", loggerNameForCustomLog, ex,
                    new Dictionary<string, object> { { "BirthDate", ex.Date } });
                break;
            case UserAccountUpdatingFailed ex:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Ошибка обновления данных";
                logger.LogError(ex, "UserAccountUpdatingFailed: User ID: {UserId}", ex.UserId);
                customLogger.Error("UserAccountUpdatingFailed", loggerNameForCustomLog, ex,
                    new Dictionary<string, object> { { "UserId", ex.UserId } });
                break;
            case TicketBookingServiceSeatNotAvailableException ex:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Место занято или недоступно";
                logger.LogWarning(ex, "TicketBookingServiceSeatNotAvailableException: Seat: {SeatMessage}", ex.Message);
                customLogger.Warn("TicketBookingServiceSeatNotAvailableException occurred", loggerNameForCustomLog, ex,
                    new Dictionary<string, object> { { "SeatMessage", ex.Message } });
                break;
            case TicketBookingServiceSeatLockNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Бронирование не найдено";
                logger.LogWarning(ex, "TicketBookingServiceSeatLockNotFoundException: SeatLock ID: {SeatLockId}", ex.SeatLockId);
                customLogger.Warn("TicketBookingServiceSeatLockNotFoundException occurred", loggerNameForCustomLog, ex,
                    new Dictionary<string, object> { { "SeatLockId", ex.SeatLockId } });
                break;
            case TicketBookingServiceTrainDepartedException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Поезд отправился";
                logger.LogWarning(ex, "TicketBookingServiceTrainDepartedException: Route ID: {routeId}. Carriage: {carriage}, Seat: {seat}", ex.RouteId, ex.Carriage, ex.Seat);
                customLogger.Warn("TicketBookingServiceTrainDepartedException occurred", loggerNameForCustomLog, ex,
                    new Dictionary<string, object> { { "RouteId", ex.RouteId }, { "Carriage", ex.Carriage }, { "Seat", ex.Seat } });
                break;
            case CarriageTemplatesNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Шаблоны вагонов не найдены";
                logger.LogWarning(ex, "CarriageTemplatesNotFoundException: Route ID: {RouteId}", ex.RouteId);
                customLogger.Warn("CarriageTemplatesNotFoundException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "RouteId", ex.RouteId } });
                break;
            case CarriageTemplateNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Шаблон вагона не найден";
                logger.LogWarning(ex, "CarriageTemplateNotFoundException: {ExceptionMessage}", ex.Message);
                customLogger.Warn("CarriageTemplateNotFoundException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "ExceptionMessage", ex.Message } });
                break;
            case ConcreteRouteNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Маршрут не найден";
                logger.LogWarning(ex, "ConcreteRouteNotFoundException: Route ID: {RouteId}", ex.RouteId);
                customLogger.Warn("ConcreteRouteNotFoundException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "RouteId", ex.RouteId } });
                break;
            case AbstractRouteNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Маршрут не найден";
                logger.LogWarning(ex, "AbstractRouteNotFoundException: Route ID: {RouteId}", ex.RouteId);
                customLogger.Warn("AbstractRouteNotFoundException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "RouteId", ex.RouteId } });
                break;
            case AbstractRouteSegmentNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Сегмент маршрута не найден";
                logger.LogWarning(ex, "AbstractRouteSegmentNotFoundException: Segment ID: {SegmentId}", ex.SegmentId);
                customLogger.Warn("AbstractRouteSegmentNotFoundException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "SegmentId", ex.SegmentId } });
                break;
            case ConcreteRouteSegmentsNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Сегменты маршрута не найдены";
                logger.LogWarning(ex, "ConcreteRouteSegmentsNotFoundException: Route ID: {RouteId}", ex.RouteId);
                customLogger.Warn("ConcreteRouteSegmentsNotFoundException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "RouteId", ex.RouteId } });
                break;
            case ConcreteRouteSegmentNotFoundException ex: // Ты использовала 'exception' вместо 'ex' здесь в оригинале
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Сегмент маршрута не найден";
                logger.LogWarning(ex, "ConcreteRouteSegmentNotFoundException: {ExceptionMessage}", ex.Message); // Исправлено на ex
                customLogger.Warn("ConcreteRouteSegmentNotFoundException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "ExceptionMessage", ex.Message } });
                break;
            case TrainNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Поезд не найден";
                logger.LogWarning(ex, "TrainNotFoundException: Train Number: {TrainNumber}", ex.Number);
                customLogger.Warn("TrainNotFoundException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "TrainNumber", ex.Number } });
                break;
            case PaymentServicePreparingFailedException ex:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Ошибка подготовки оплаты";
                logger.LogError(ex, "PaymentServicePreparingFailedException: SeatLock ID: {SeatLockId}", ex.SeatLockId);
                customLogger.Error("PaymentServicePreparingFailedException", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "SeatLockId", ex.SeatLockId } });
                break;
            case PaymentServicePaymentFailedException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Оплата не выполнена";
                logger.LogWarning(ex, "PaymentServicePaymentFailedException: SeatLock ID: {SeatLockId}", ex.SeatLockId);
                customLogger.Warn("PaymentServicePaymentFailedException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "SeatLockId", ex.SeatLockId } });
                break;
            case PaymentServiceTicketNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Билет не найден";
                logger.LogWarning(ex, "PaymentServiceTicketNotFoundException: Ticket ID: {TicketId}, User ID: {UserAccountId}", ex.TicketId, ex.UserAccountId);
                customLogger.Warn("PaymentServiceTicketNotFoundException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "TicketId", ex.TicketId }, { "UserAccountId", ex.UserAccountId } });
                break;
            case PaymentServiceTicketNotPayedException ex:
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                problemDetails.Status = StatusCodes.Status403Forbidden;
                problemDetails.Title = "Билет не оплачен";
                logger.LogWarning(ex, "PaymentServiceTicketNotPayedException: Ticket ID: {TicketId}", ex.TicketId);
                customLogger.Warn("PaymentServiceTicketNotPayedException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "TicketId", ex.TicketId } });
                break;
            case SeatLockExpiredException ex:
                context.Response.StatusCode = StatusCodes.Status410Gone;
                problemDetails.Status = StatusCodes.Status410Gone;
                problemDetails.Title = "Время бронирования истекло";
                logger.LogWarning(ex, "SeatLockExpiredException: SeatLock ID: {SeatLockId}", ex.SeatLockId);
                customLogger.Warn("SeatLockExpiredException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "SeatLockId", ex.SeatLockId } });
                break;
            case SeatLockNotActiveException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Бронирование не активно";
                logger.LogWarning(ex, "SeatLockNotActiveException: SeatLock ID: {SeatLockId}", ex.SeatLockId);
                customLogger.Warn("SeatLockNotActiveException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "SeatLockId", ex.SeatLockId } });
                break;
            case SeatLockNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Бронирование не найдено";
                logger.LogWarning(ex, "SeatLockNotFoundException: SeatLock ID: {SeatLockId}, User ID: {UserAccountId}", ex.SeatLockId, ex.UserAccountId);
                customLogger.Warn("SeatLockNotFoundException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "SeatLockId", ex.SeatLockId }, { "UserAccountId", ex.UserAccountId } });
                break;
            case CarriageAvailabilityUpdateServiceSeatNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Место не найдено";
                logger.LogWarning(ex, "CarriageAvailabilityUpdateServiceSeatNotFoundException: Seat: {SeatNumber}", ex.Seat);
                customLogger.Warn("CarriageAvailabilityUpdateServiceSeatNotFoundException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "SeatNumber", ex.Seat } });
                break;
            case CarriageAvailabilityUpdateServiceSeatAlreadyBookedException ex:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Место уже занято";
                logger.LogWarning(ex, "CarriageAvailabilityUpdateServiceSeatAlreadyBookedException: Seat: {SeatNumber}", ex.Seat);
                customLogger.Warn("CarriageAvailabilityUpdateServiceSeatAlreadyBookedException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "SeatNumber", ex.Seat } });
                break;
            case CarriageAvailabilityUpdateServiceSeatAlreadyFreeException ex:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Место уже свободно";
                logger.LogWarning(ex, "CarriageAvailabilityUpdateServiceSeatAlreadyFreeException: Seat: {SeatNumber}", ex.Seat);
                customLogger.Warn("CarriageAvailabilityUpdateServiceSeatAlreadyFreeException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "SeatNumber", ex.Seat } });
                break;
            case TicketNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Билет не найден";
                logger.LogWarning(ex, "TicketNotFoundException: Ticket ID: {TicketId}", ex.TicketId);
                customLogger.Warn("TicketNotFoundException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "TicketId", ex.TicketId } });
                break;
            case CarriageSeatServiceCarriageAvailabilityNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Вагоны не доступны";
                logger.LogWarning(ex, "CarriageSeatServiceCarriageAvailabilityNotFoundException: Route ID: {RouteId}, Segments: {StartSegment}-{EndSegment}", ex.RouteId, ex.StartSegment, ex.EndSegment);
                customLogger.Warn("CarriageSeatServiceCarriageAvailabilityNotFoundException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "RouteId", ex.RouteId }, { "StartSegment", ex.StartSegment }, { "EndSegment", ex.EndSegment } });
                break;
            case RouteSearchServicePathCreatingFailedException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Ошибка построения маршрута";
                logger.LogWarning(ex, "RouteSearchServicePathCreatingFailedException: Error: {ErrorMessage}", ex.Message);
                customLogger.Warn("RouteSearchServicePathCreatingFailedException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "ErrorMessage", ex.Message } });
                break;
            case StationNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Станция не найдена";
                logger.LogWarning(ex, "StationNotFoundException: Station ID: {StationId}", ex.StationId);
                customLogger.Warn("StationNotFoundException occurred", loggerNameForCustomLog, ex,
                     new Dictionary<string, object> { { "StationId", ex.StationId } });
                break;
            case HttpRequestException httpEx:
                context.Response.StatusCode = (int)(httpEx.StatusCode ?? HttpStatusCode.BadRequest);
                problemDetails.Status = context.Response.StatusCode;
                problemDetails.Title = httpEx.Message ?? "Ошибка при выполнении внешнего запроса.";
                logger.LogWarning(httpEx, "HttpRequestException occurred: StatusCode: {StatusCode}, Message: {Message}", httpEx.StatusCode, httpEx.Message);
                customLogger.Warn("HttpRequestException occurred", loggerNameForCustomLog, httpEx,
                     new Dictionary<string, object> { { "StatusCode", httpEx.StatusCode?.ToString() ?? "N/A" }, { "Message", httpEx.Message } });
                break;
            case ArgumentException argEx: // System.ArgumentException
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Некорректные данные запроса."; // Общее сообщение для пользователя
                logger.LogWarning(argEx, "ArgumentException: ParameterName: {ParamName}, Message: {Message}", argEx.ParamName, argEx.Message);
                customLogger.Warn("ArgumentException occurred", loggerNameForCustomLog, argEx,
                    new Dictionary<string, object> { { "ParameterName", argEx.ParamName }, { "Details", argEx.Message } });
                break;
            default: // Необработанные исключения
                // Title и StatusCode уже установлены по умолчанию для 500
                logger.LogError(exception, "An unhandled exception type ({ExceptionType}) was processed by the default case in ExceptionHandlingMiddleware.", exception.GetType().Name);
                customLogger.Error("Unhandled exception", loggerNameForCustomLog, exception,
                    new Dictionary<string, object> { { "ExceptionType", exception.GetType().Name } });
                break;
        }

        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(problemDetails,
            new JsonSerializerOptions { WriteIndented = env.IsDevelopment() }));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}