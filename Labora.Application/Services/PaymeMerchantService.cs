using System.Text.Json;
using Labora.Application.DTOs.Payments.Payme;
using Labora.Application.Interfaces;
using Labora.Domain.Exceptions;

namespace Labora.Application.Services;

public class PaymeMerchantService : IPaymeMerchantService
{
    private readonly IPaymeAuthenticator _paymeAuthenticator;

    public PaymeMerchantService(IPaymeAuthenticator paymeAuthenticator)
    {
        _paymeAuthenticator = paymeAuthenticator;
    }

    public async Task<PaymeRpcResponse<object?>> HandleRequestAsync(string rawRequestBody, string? authorizationHeader)
    {
        // Declared outside the try block (and mutated, not redeclared, inside it) so the outer
        // catch-all below can still echo it back - an exception the inner catches don't anticipate
        // (e.g. a stubbed handler throwing NotImplementedException) must not lose the request id.
        long? requestId = null;

        try
        {
            if (!_paymeAuthenticator.Validate(authorizationHeader))
            {
                return ErrorResponse(requestId, PaymeErrorCodes.InsufficientPrivileges, "Insufficient privileges to perform this method.");
            }

            JsonElement root;
            try
            {
                using JsonDocument document = JsonDocument.Parse(rawRequestBody);
                // Clone so the element remains valid after the JsonDocument above is disposed -
                // JsonDocument.Dispose() invalidates any JsonElement obtained from it otherwise.
                root = document.RootElement.Clone();
            }
            catch (JsonException)
            {
                return ErrorResponse(requestId, PaymeErrorCodes.JsonParseError, "Malformed JSON in request body.");
            }

            requestId = TryGetId(root);

            if (!TryParseEnvelope(root, out string method, out JsonElement paramsElement))
            {
                return ErrorResponse(requestId, PaymeErrorCodes.InvalidRpcRequest, "Invalid JSON-RPC request.");
            }

            try
            {
                object? result = await DispatchAsync(method, paramsElement);
                return new PaymeRpcResponse<object?> { Result = result, Id = requestId ?? 0 };
            }
            catch (PaymeRpcException ex)
            {
                return ErrorResponse(requestId, ex.PaymeErrorCode, ex.Message);
            }
            catch (JsonException)
            {
                return ErrorResponse(requestId, PaymeErrorCodes.InvalidRpcRequest, "Invalid method parameters.");
            }
        }
        catch (Exception)
        {
            // Defense in depth: this method must never throw - Payme's protocol requires HTTP 200
            // with a JSON-RPC-shaped body on every response, even for failures not anticipated above.
            return ErrorResponse(requestId, PaymeErrorCodes.InternalSystemError, "An internal error occurred.");
        }
    }

    public Task<CheckPerformTransactionResponseDto> CheckPerformTransactionAsync(CheckPerformTransactionRequestDto request)
        => throw new NotImplementedException("CheckPerformTransaction business logic is implemented in a later batch.");

    public Task<CreateTransactionResponseDto> CreateTransactionAsync(CreateTransactionRequestDto request)
        => throw new NotImplementedException("CreateTransaction business logic is implemented in a later batch.");

    public Task<PerformTransactionResponseDto> PerformTransactionAsync(PerformTransactionRequestDto request)
        => throw new NotImplementedException("PerformTransaction business logic is implemented in a later batch.");

    public Task<CancelTransactionResponseDto> CancelTransactionAsync(CancelTransactionRequestDto request)
        => throw new NotImplementedException("CancelTransaction business logic is implemented in a later batch.");

    public Task<CheckTransactionResponseDto> CheckTransactionAsync(CheckTransactionRequestDto request)
        => throw new NotImplementedException("CheckTransaction business logic is implemented in a later batch.");

    public Task<GetStatementResponseDto> GetStatementAsync(GetStatementRequestDto request)
        => throw new NotImplementedException("GetStatement business logic is implemented in a later batch.");

    private Task<object?> DispatchAsync(string method, JsonElement paramsElement) => method switch
    {
        "CheckPerformTransaction" => InvokeAsync<CheckPerformTransactionRequestDto, CheckPerformTransactionResponseDto>(CheckPerformTransactionAsync, paramsElement),
        "CreateTransaction" => InvokeAsync<CreateTransactionRequestDto, CreateTransactionResponseDto>(CreateTransactionAsync, paramsElement),
        "PerformTransaction" => InvokeAsync<PerformTransactionRequestDto, PerformTransactionResponseDto>(PerformTransactionAsync, paramsElement),
        "CancelTransaction" => InvokeAsync<CancelTransactionRequestDto, CancelTransactionResponseDto>(CancelTransactionAsync, paramsElement),
        "CheckTransaction" => InvokeAsync<CheckTransactionRequestDto, CheckTransactionResponseDto>(CheckTransactionAsync, paramsElement),
        "GetStatement" => InvokeAsync<GetStatementRequestDto, GetStatementResponseDto>(GetStatementAsync, paramsElement),
        _ => throw new PaymeRpcException(PaymeErrorCodes.MethodNotFound, $"Method '{method}' not found.")
    };

    private static async Task<object?> InvokeAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler, JsonElement paramsElement)
    {
        TRequest request = JsonSerializer.Deserialize<TRequest>(paramsElement.GetRawText())
            ?? throw new PaymeRpcException(PaymeErrorCodes.InvalidRpcRequest, "Invalid method parameters.");

        return await handler(request);
    }

    private static bool TryParseEnvelope(JsonElement root, out string method, out JsonElement paramsElement)
    {
        method = string.Empty;
        paramsElement = default;

        if (root.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!root.TryGetProperty("method", out JsonElement methodElement) || methodElement.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        if (!root.TryGetProperty("id", out JsonElement idElement) || idElement.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        if (!root.TryGetProperty("params", out JsonElement paramsCandidate) || paramsCandidate.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        method = methodElement.GetString()!;
        paramsElement = paramsCandidate;
        return true;
    }

    private static long? TryGetId(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("id", out JsonElement idElement) &&
            idElement.ValueKind == JsonValueKind.Number &&
            idElement.TryGetInt64(out long id))
        {
            return id;
        }

        return null;
    }

    private static PaymeRpcResponse<object?> ErrorResponse(long? id, int code, string message)
    {
        return new PaymeRpcResponse<object?>
        {
            // Payme requests always carry a numeric "id" per official docs, so this envelope's Id
            // stays non-nullable; a request malformed enough to lack a readable id has no meaningful
            // value to echo back, so 0 is used as the closest available default.
            Id = id ?? 0,
            Error = new PaymeError
            {
                Code = code,
                Message = JsonSerializer.SerializeToElement(message)
            }
        };
    }
}
