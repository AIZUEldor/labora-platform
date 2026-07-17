namespace Labora.Domain.Exceptions;

public class OtpMaxAttemptsExceededException : Exception
{
    public OtpMaxAttemptsExceededException(string message) : base(message) { }
    public OtpMaxAttemptsExceededException(string message, Exception innerException) : base(message, innerException) { }
}
