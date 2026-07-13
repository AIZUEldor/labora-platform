namespace Labora.Domain.Exceptions;

public class OtpConcurrencyException : Exception
{
    public OtpConcurrencyException(string message) : base(message) { }
    public OtpConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
}
