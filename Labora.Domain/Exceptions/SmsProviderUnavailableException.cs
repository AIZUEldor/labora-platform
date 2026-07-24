namespace Labora.Domain.Exceptions;

public class SmsProviderUnavailableException : Exception
{
    public SmsProviderUnavailableException(string message) : base(message) { }
    public SmsProviderUnavailableException(string message, Exception innerException) : base(message, innerException) { }
}
