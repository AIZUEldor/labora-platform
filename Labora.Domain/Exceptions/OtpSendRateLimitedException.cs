namespace Labora.Domain.Exceptions;

public class OtpSendRateLimitedException : Exception
{
    public DateTime RetryAfterUtc { get; }

    public OtpSendRateLimitedException(DateTime retryAfterUtc, string message) : base(message)
    {
        RetryAfterUtc = retryAfterUtc;
    }
}
