namespace RailwayApp.Domain;

public class UserServiceException(string message) : Exception(message);
public class UserServiceEmailAlreadyExistsException(string email)
    : UserServiceException($"User with Email '{email}' already exists.");

public class UserServiceUserNotFoundException(Guid userId) 
    : UserServiceException($"User with ID '{userId}' not found.");

public class UserServiceUserBlockedException(Guid userId) 
    : UserServiceException($"User with ID '{userId}' not found.");

public class UserServiceInvalidPasswordException(string email)
    : UserServiceException($"Invalid password for user with email '{email}'."); 
    
public class TicketBookingServiceException(string message) : Exception(message);

public class TicketBookingServiceSeatNotAvailableException(string message) : 
    TicketBookingServiceException($"chosen place {message} is not available");
    
public class TicketBookingServiceSeatLockNotFoundException(Guid id) : 
    TicketBookingServiceException($"seat lock {id} not found");

public class CarriageTemplateNotFoundException(string message) : Exception(message);

public class PaymentServiceException(string message) : Exception(message);
public class PaymentServicePaymentFailedException(Guid seatLockId) :
    PaymentServiceException($"payment for seatLock {seatLockId} failed");

public class SeatLockException(string message) : Exception(message);
public class SeatLockExpiredException(Guid id) : SeatLockException($"SeatLock {id} not found or expired");
public class SeatLockNotActiveException(Guid id) : SeatLockException($"SeatLock {id} not active");