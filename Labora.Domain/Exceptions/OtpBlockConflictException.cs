namespace Labora.Domain.Exceptions;

public class OtpBlockConflictException : Exception
{
    public OtpBlockConflictException(string message) : base(message) { }
    public OtpBlockConflictException(string message, Exception innerException) : base(message, innerException) { }
}
