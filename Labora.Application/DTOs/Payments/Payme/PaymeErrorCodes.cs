namespace Labora.Application.DTOs.Payments.Payme;

/// <summary>
/// Numeric JSON-RPC error codes confirmed from the official Payme Merchant API documentation
/// (developer.help.paycom.uz). Codes not listed here must not be assumed - consult official
/// documentation before adding new values.
/// </summary>
public static class PaymeErrorCodes
{
    // Method-specific errors
    public const int InvalidAmount = -31001;
    public const int TransactionNotFound = -31003;
    public const int OrderAlreadyCompleted = -31007;
    public const int OperationNotAllowed = -31008;

    // Account validation errors occupy a documented range rather than discrete codes.
    public const int AccountErrorRangeStart = -31050;
    public const int AccountErrorRangeEnd = -31099;

    // General JSON-RPC transport/protocol errors
    public const int HttpMethodNotPost = -32300;
    public const int JsonParseError = -32700;
    public const int InvalidRpcRequest = -32600;
    public const int MethodNotFound = -32601;
    public const int InsufficientPrivileges = -32504;
    public const int InternalSystemError = -32400;

    // Range is documented as -31050..-31099, i.e. AccountErrorRangeStart is numerically the larger
    // (less negative) bound - Math.Min/Max keeps the comparison correct regardless of which constant
    // is textually "start" vs "end".
    public static bool IsAccountError(int code) =>
        code >= Math.Min(AccountErrorRangeStart, AccountErrorRangeEnd)
        && code <= Math.Max(AccountErrorRangeStart, AccountErrorRangeEnd);
}
