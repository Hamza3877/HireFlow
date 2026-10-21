namespace HireFlow.Application.Exceptions;

// Thrown when login details are wrong (becomes HTTP 401).
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}
