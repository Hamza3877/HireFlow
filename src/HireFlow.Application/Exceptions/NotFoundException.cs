namespace HireFlow.Application.Exceptions;

// Thrown when something we look for does not exist (becomes HTTP 404).
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
