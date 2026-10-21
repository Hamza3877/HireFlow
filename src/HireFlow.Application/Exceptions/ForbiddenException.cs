namespace HireFlow.Application.Exceptions;

// Thrown when the user's role is not allowed to do the action (becomes HTTP 403).
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
