namespace Labora.Domain.Exceptions;

public class SmsSendException : Exception
{
    public SmsSendException(string message) : base(message) { }
    public SmsSendException(string message, Exception innerException) : base(message, innerException) { }
}
