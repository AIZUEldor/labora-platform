namespace Labora.Domain.Exceptions;

public class OtpExpiredException : Exception
{
    public OtpExpiredException(string message) : base(message) { }
    public OtpExpiredException(string message, Exception innerException) : base(message, innerException) { }
}
