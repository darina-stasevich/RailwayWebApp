namespace RailwayApp.Domain;

public class UserAccountException(string message) : Exception(message);

public class UserAccountEmailAlreadyExistsException(string email)
    : UserAccountException($"User with Email '{email}' already exists.")
{
    public string Email { get; } = email;
}

public class UserAccountUserNotFoundException(Guid userId)
    : UserAccountException($"User with ID '{userId}' not found.")
{
    public Guid UserId { get; } = userId;
}

public class UserAccountUserBlockedException(Guid userId)
    : UserAccountException($"User with ID '{userId}' not found.")
{
    public Guid UserId { get; } = userId;
}

public class UserAccountInvalidPasswordException(string email)
    : UserAccountException($"Invalid password for user with email '{email}'.")
{
    public string Email { get; } = email;
}

public class UserAccountUpdatingFailed(Guid id)
    : UserAccountException($"updating data of user {id} failed")
{
    public Guid UserId { get; } = id;
}

public class TicketBookingServiceException(string message) : Exception(message);

public class TicketBookingServiceSeatNotAvailableException(
    int seat,
    Guid routeId,
    int startSegmentNumber,
    int endSegmentNumber) :
    TicketBookingServiceException($"Seat {seat} is not available for ConcreteRouteId {routeId}, " +
                                  $"StartSegmentNumber {startSegmentNumber}, EndSegmentNumber {endSegmentNumber}")
{
    public int Seat { get; } = seat;
    public Guid RouteId { get; } = routeId;
    public int StartSegmentNumber { get; } = startSegmentNumber;
    public int EndSegmentNumber { get; } = endSegmentNumber;
}

public class TicketBookingServiceSeatLockNotFoundException(Guid id) :
    TicketBookingServiceException($"seat lock {id} not found")
{
    public Guid SeatLockId { get; } = id;
}

public class TicketBookingServiceTrainDepartedException(Guid routeId, int carriage, int seat) :
    TicketBookingServiceException(
        $"Train already departed. Seat {seat} in carriage {carriage} is locked for booking in route {routeId}")
{
    public Guid RouteId { get; } = routeId;
    public int Carriage { get; } = carriage;
    public int Seat { get; } = seat;
}

public class TicketBookingServiceRouteSegmentNotFound(Guid routeId, int segmentNumber) :
    TicketBookingServiceException($"concrete route segment with number {segmentNumber} not found for route {routeId}");
public class CarriageTemplateException(string message) : Exception(message);

public class CarriageTemplatesNotFoundException(Guid routeId)
    : CarriageTemplateException($"carriage templates not found for route {routeId}")
{
    public Guid RouteId { get; } = routeId;
}

public class CarriageTemplateNotFoundException : CarriageTemplateException
{
    public CarriageTemplateNotFoundException(Guid id, Guid routeId) : base(
        $"carriage template {id} not found for route {routeId}")
    {
        CarriageTemplateId = id;
        RouteId = routeId;
    }

    public CarriageTemplateNotFoundException(int carriageNumber, Guid routeId) : base(
        $"carriage template {carriageNumber} not found for route {routeId}")
    {
        CarriageNumber = carriageNumber;
        RouteId = routeId;
    }

    public CarriageTemplateNotFoundException(
        Guid carriageTemplateId,
        Guid routeId,
        int startSegment,
        int endSegment)
        : base(
            $"carriage template {carriageTemplateId} not found for route {routeId} for segments {startSegment} - {endSegment}")
    {
        CarriageTemplateId = carriageTemplateId;
        RouteId = routeId;
    }

    public Guid CarriageTemplateId { get; }
    public Guid RouteId { get; }

    public int CarriageNumber { get; }
}

public class ConcreteRouteNotFoundException(Guid id) : Exception($"concrete route with id {id} not found")
{
    public Guid RouteId { get; } = id;
}

public class AbstractRouteNotFoundException(Guid id) : Exception($"abstract route with id {id} not found")
{
    public Guid RouteId { get; } = id;
}

public class AbstractRouteSegmentNotFoundException(Guid id) : Exception($"abstract route segment {id} not found")
{
    public Guid SegmentId { get; } = id;
}

public class ConcreteRouteSegmentsNotFoundException(Guid routeId)
    : Exception($"concrete route segments not found for route {routeId}")
{
    public Guid RouteId { get; } = routeId;
}

public class TrainNotFoundException(string number) : Exception($"train {number} not found")
{
    public string Number { get; } = number;
}

public class PaymentServiceException(string message) : Exception(message);

public class PaymentServicePreparingFailedException(Guid seatLockId)
    : PaymentServiceException($"preparing of seatLock {seatLockId} for payment failed")
{
    public Guid SeatLockId { get; } = seatLockId;
}

public class PaymentServicePaymentFailedException(Guid seatLockId) :
    PaymentServiceException($"payment for seatLock {seatLockId} failed")
{
    public Guid SeatLockId { get; } = seatLockId;
}

public class PaymentServiceTicketNotFoundException(Guid ticketId, Guid userAccountId)
    : PaymentServiceException($"ticket {ticketId} not found for user {userAccountId}")
{
    public Guid TicketId { get; } = ticketId;
    public Guid UserAccountId { get; } = userAccountId;
}

public class PaymentServiceTicketNotPayedException(Guid ticketId)
    : PaymentServiceException($"Ticket {ticketId} not paid")
{
    public Guid TicketId { get; } = ticketId;
}

public class SeatLockException(string message) : Exception(message);

public class SeatLockExpiredException(Guid id) : SeatLockException($"SeatLock {id} not found or expired")
{
    public Guid SeatLockId { get; } = id;
}

public class SeatLockNotActiveException(Guid id) : SeatLockException($"SeatLock {id} not active")
{
    public Guid SeatLockId { get; } = id;
}

public class SeatLockNotFoundException(Guid seatLockId, Guid userAccountId)
    : SeatLockException($"seatLock {seatLockId} not found for user {userAccountId}")
{
    public Guid SeatLockId { get; } = seatLockId;
    public Guid UserAccountId { get; } = userAccountId;
}

public class CarriageAvailabilityUpdateServiceException(string message) : Exception(message);

public class CarriageAvailabilityUpdateServiceSeatNotFoundException(int seat)
    : CarriageAvailabilityUpdateServiceException($"seat {seat} not found")
{
    public int Seat { get; } = seat;
}

public class CarriageAvailabilityUpdateServiceSeatAlreadyBookedException(int seat)
    : CarriageAvailabilityUpdateServiceException($"seat {seat} already payed")
{
    public int Seat { get; } = seat;
}

public class CarriageAvailabilityUpdateServiceSeatAlreadyFreeException(int seat)
    : CarriageAvailabilityUpdateServiceException($"seat {seat} already free")
{
    public int Seat { get; } = seat;
}

public class TicketException(string message) : Exception(message);

public class TicketNotFoundException(Guid id) : TicketException($"Ticket {id} not found")
{
    public Guid TicketId { get; } = id;
}

public class CarriageSeatServiceException(string message) : Exception(message);

public class CarriageSeatServiceCarriageAvailabilityNotFoundException(Guid routeId, int startSegment, int endSegment)
    : CarriageSeatServiceException($"No carriage found for {routeId} for segments {startSegment} - {endSegment}")
{
    public Guid RouteId { get; } = routeId;
    public int StartSegment { get; } = startSegment;
    public int EndSegment { get; } = endSegment;
}

public class RouteSearchServiceException(string message) : Exception(message);

public class RouteSearchServicePathCreatingFailedException(string message) : RouteSearchServiceException(message);

public class StationExceptions(string message) : Exception(message);

public class StationNotFoundException(Guid id) : StationExceptions($"station {id} not found")
{
    public Guid StationId { get; set; } = id;
}

public abstract class AdminOperationException : Exception
{
    protected AdminOperationException(string message) : base(message) { }
    protected AdminOperationException(string message, Exception innerException) : base(message, innerException) { }
}

public class AdminResourceNotFoundException : AdminOperationException
{
    public string ResourceName { get; }
    public object ResourceId { get; }

    public AdminResourceNotFoundException(string resourceName, object resourceId)
        : base($"Resource '{resourceName}' with ID '{resourceId}' was not found.")
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }
}

public class AdminDataConflictException : AdminOperationException
{ 
    public AdminDataConflictException(string message) : base(message) { }
}

public class AdminValidationException : AdminOperationException
{
   public AdminValidationException(string message) : base(message) { }
}