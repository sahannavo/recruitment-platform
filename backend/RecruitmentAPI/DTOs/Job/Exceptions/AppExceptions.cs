namespace RecruitmentAPI.Exceptions;

/// <summary>
/// Base exception for application-specific errors.
/// </summary>
public abstract class AppException : Exception
{
    public int StatusCode { get; }

    protected AppException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// Thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, StatusCodes.Status404NotFound) { }
}

/// <summary>
/// Thrown when the request contains invalid data.
/// </summary>
public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message, StatusCodes.Status400BadRequest) { }
}

/// <summary>
/// Thrown when the caller lacks authentication credentials.
/// </summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message, StatusCodes.Status401Unauthorized) { }
}

/// <summary>
/// Thrown when the caller is authenticated but lacks permission.
/// </summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message, StatusCodes.Status403Forbidden) { }
}

