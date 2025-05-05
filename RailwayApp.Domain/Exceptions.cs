namespace RailwayApp.Domain;

public class UserServiceException : Exception
{
    public UserServiceException(string message) : base(message) { }
    public UserServiceException(string message, Exception innerException) : base(message, innerException) { }
}

public class UserServiceEmailAlreadyExistsException(string email)
    : UserServiceException($"User with Email '{email}' already exists.");

public class UserServiceUserNotFoundException(Guid userId) 
    : UserServiceException($"User with ID '{userId}' not found.");

public class UserServiceUserBlockedException(Guid userId) 
    : UserServiceException($"User with ID '{userId}' not found.");

public class UserServiceInvalidPasswordException(string email)
    : UserServiceException($"Invalid password for user with email '{email}'."); 