namespace Labora.Domain.Exceptions;

/// <summary>
/// Carries a Payme JSON-RPC error code (see Labora.Application's PaymeErrorCodes) out of a business
/// method so the merchant API adapter can translate it into a PaymeError response. Domain does not
/// reference PaymeErrorCodes directly (Application/Infrastructure depend on Domain, not vice versa) -
/// callers pass the raw int constant.
/// </summary>
public class PaymeRpcException : Exception
{
    public int PaymeErrorCode { get; }

    public PaymeRpcException(int paymeErrorCode, string message) : base(message)
    {
        PaymeErrorCode = paymeErrorCode;
    }
}
