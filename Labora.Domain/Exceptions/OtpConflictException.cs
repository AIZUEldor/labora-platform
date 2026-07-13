namespace Labora.Domain.Exceptions;

public class OtpConflictException : Exception
{
    public OtpConflictException(string message) : base(message) { }
    public OtpConflictException(string message, Exception innerException) : base(message, innerException) { }
}
