namespace Labora.Domain.Exceptions;

public class PaymeConflictException : Exception
{
    public PaymeConflictException(string message) : base(message) { }
    public PaymeConflictException(string message, Exception innerException) : base(message, innerException) { }
}
