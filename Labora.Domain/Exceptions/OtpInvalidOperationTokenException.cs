namespace Labora.Domain.Exceptions;

public class OtpInvalidOperationTokenException : Exception
{
    public OtpInvalidOperationTokenException(string message) : base(message) { }
    public OtpInvalidOperationTokenException(string message, Exception innerException) : base(message, innerException) { }
}
