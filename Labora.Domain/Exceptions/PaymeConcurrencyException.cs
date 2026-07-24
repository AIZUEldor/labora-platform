namespace Labora.Domain.Exceptions;

public class PaymeConcurrencyException : Exception
{
    public PaymeConcurrencyException(string message) : base(message) { }
    public PaymeConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
}
