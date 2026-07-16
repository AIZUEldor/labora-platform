namespace Labora.Domain.Exceptions;

public class OtpBlockConcurrencyException : Exception
{
    public OtpBlockConcurrencyException(string message) : base(message) { }
    public OtpBlockConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
}
