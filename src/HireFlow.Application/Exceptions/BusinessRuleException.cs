namespace HireFlow.Application.Exceptions;

// Thrown when a business rule is broken (becomes HTTP 409).
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
