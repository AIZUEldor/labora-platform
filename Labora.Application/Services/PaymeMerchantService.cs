using System.Text.Json;
using Labora.Application.DTOs.Payments.Payme;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Exceptions;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class PaymeMerchantService : IPaymeMerchantService
{
    private const int MaxWriteConflictRetries = 3;

    // Confirmed: Payme's "Created" transaction state is 1. Consistent with CancelTransaction's
    // official documentation example, which shows "state": -2 for a transaction cancelled after
    // being performed - a signed-integer state scheme where 1 = Created, 2 = Performed.
    private const int PaymeCreatedStateCode = 1;

    private readonly IPaymeAuthenticator _paymeAuthenticator;
    private readonly IPaymentOrderRepository _paymentOrderRepository;
    private readonly IPaymeTransactionRepository _paymeTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymeMerchantService(
        IPaymeAuthenticator paymeAuthenticator,
        IPaymentOrderRepository paymentOrderRepository,
        IPaymeTransactionRepository paymeTransactionRepository,
        IUnitOfWork unitOfWork)
    {
        _paymeAuthenticator = paymeAuthenticator;
        _paymentOrderRepository = paymentOrderRepository;
        _paymeTransactionRepository = paymeTransactionRepository;
        _unitOfWork = unitOfWork;
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

    public async Task<CheckPerformTransactionResponseDto> CheckPerformTransactionAsync(CheckPerformTransactionRequestDto request)
    {
        Guid orderId = ParseOrderId(request.Account);
        PaymentOrder? order = await _paymentOrderRepository.GetByIdAsync(orderId);
        ValidateOrderForPayme(order, request.Amount);

        return new CheckPerformTransactionResponseDto { Allow = true };
    }

    public async Task<CreateTransactionResponseDto> CreateTransactionAsync(CreateTransactionRequestDto request)
    {
        for (int attempt = 1; attempt <= MaxWriteConflictRetries; attempt++)
        {
            try
            {
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    // Idempotency check first, before any validation: a repeated CreateTransaction for
                    // an id already created must replay the stored result, not re-validate from scratch
                    // (matches the confirmed official-docs behavior: "basic check, return existing state").
                    PaymeTransaction? existing = await _paymeTransactionRepository.GetByPaymeTransactionIdForUpdateAsync(request.Id);
                    if (existing is not null)
                    {
                        return BuildReplayResponse(existing, request);
                    }

                    Guid orderId = ParseOrderId(request.Account);
                    PaymentOrder? order = await _paymentOrderRepository.GetByIdForUpdateAsync(orderId);
                    ValidateOrderForPayme(order, request.Amount);

                    // Enforce one Payme transaction per PaymentOrder - a different Payme transaction id
                    // must never be allowed to attach to an order that already has one.
                    PaymeTransaction? attachedToOrder = await _paymeTransactionRepository.GetByPaymentOrderIdAsync(order!.Id);
                    if (attachedToOrder is not null)
                    {
                        throw new PaymeRpcException(PaymeErrorCodes.OperationNotAllowed,
                            "A different Payme transaction is already attached to this payment order.");
                    }

                    PaymeTransaction transaction = new()
                    {
                        PaymentOrderId = order.Id,
                        PaymeTransactionId = request.Id,
                        AccountReference = request.Account.OrderId,
                        PaymeTransactionTime = request.Time,
                        RequestedAmountTiyin = request.Amount,
                        MerchantCreateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        InternalStatus = PaymeTransactionInternalStatus.Created,
                        PaymeStateCode = PaymeCreatedStateCode
                    };

                    // PaymeTransaction is created before PaymentOrder is touched, so a failure here
                    // (e.g. a unique-index conflict) never leaves PaymentOrder.ProviderOrderId
                    // partially updated - and both writes still commit/rollback together as one
                    // IUnitOfWork transaction regardless.
                    PaymeTransaction created = await _paymeTransactionRepository.AddAsync(transaction);

                    order.ProviderOrderId = request.Id;
                    await _paymentOrderRepository.UpdateAsync(order);

                    return BuildResponse(created);
                });
            }
            catch (Exception ex) when (IsRetryableConflict(ex))
            {
                // A DB-level conflict (unique index or xmin) aborted the whole transaction - UnitOfWork
                // has already rolled it back. If attempts remain, retry the entire delegate from
                // scratch with fresh reads: a concurrent CreateTransaction for the same Payme id
                // resolves to the idempotent-replay path once the winner's row becomes visible. On the
                // final attempt, translate rather than let PaymeConflictException/PaymeConcurrencyException
                // escape as an unhandled type - this method must only ever return a result or throw
                // PaymeRpcException.
                if (attempt >= MaxWriteConflictRetries)
                {
                    throw new PaymeRpcException(PaymeErrorCodes.InternalSystemError,
                        "Unable to process CreateTransaction after repeated conflicts.");
                }
            }
        }

        throw new PaymeRpcException(PaymeErrorCodes.InternalSystemError,
            "Unable to process CreateTransaction after repeated conflicts.");
    }

    public Task<PerformTransactionResponseDto> PerformTransactionAsync(PerformTransactionRequestDto request)
        => throw new NotImplementedException("PerformTransaction business logic is implemented in a later batch.");

    public Task<CancelTransactionResponseDto> CancelTransactionAsync(CancelTransactionRequestDto request)
        => throw new NotImplementedException("CancelTransaction business logic is implemented in a later batch.");

    public Task<CheckTransactionResponseDto> CheckTransactionAsync(CheckTransactionRequestDto request)
        => throw new NotImplementedException("CheckTransaction business logic is implemented in a later batch.");

    public Task<GetStatementResponseDto> GetStatementAsync(GetStatementRequestDto request)
        => throw new NotImplementedException("GetStatement business logic is implemented in a later batch.");

    private static bool IsRetryableConflict(Exception ex) =>
        ex is PaymeConcurrencyException or PaymeConflictException;

    private static Guid ParseOrderId(PaymeAccountDto account)
    {
        if (string.IsNullOrWhiteSpace(account.OrderId) || !Guid.TryParse(account.OrderId, out Guid orderId))
        {
            throw new PaymeRpcException(PaymeErrorCodes.AccountErrorRangeStart, "Invalid or missing account.order_id.");
        }

        return orderId;
    }

    private static void ValidateOrderForPayme(PaymentOrder? order, long requestedAmountTiyin)
    {
        if (order is null || order.Provider != PaymentProvider.Payme)
        {
            throw new PaymeRpcException(PaymeErrorCodes.AccountErrorRangeStart, "Payment order not found.");
        }

        bool isExpired = order.ExpiresAt is not null && order.ExpiresAt <= DateTime.UtcNow;
        if (order.Status != PaymentOrderStatus.Pending || isExpired)
        {
            throw new PaymeRpcException(PaymeErrorCodes.AccountErrorRangeStart, "Payment order is not payable.");
        }

        long expectedAmountTiyin = ToTiyin(order.Amount);
        if (requestedAmountTiyin != expectedAmountTiyin)
        {
            throw new PaymeRpcException(PaymeErrorCodes.InvalidAmount, "Requested amount does not match the payment order.");
        }
    }

    // PaymentOrder.Amount is a decimal(18,2) column - by the time any value is read back from the
    // repository it can have at most 2 decimal places (tiyin granularity), so this conversion is
    // exact; no rounding is needed.
    private static long ToTiyin(decimal soum) => (long)(soum * 100m);

    private static CreateTransactionResponseDto BuildReplayResponse(PaymeTransaction existing, CreateTransactionRequestDto request)
    {
        Guid orderId = ParseOrderId(request.Account);

        if (existing.PaymentOrderId != orderId || existing.RequestedAmountTiyin != request.Amount)
        {
            throw new PaymeRpcException(PaymeErrorCodes.OperationNotAllowed,
                "This Payme transaction id was already created with different order or amount parameters.");
        }

        return BuildResponse(existing);
    }

    private static CreateTransactionResponseDto BuildResponse(PaymeTransaction transaction)
    {
        return new CreateTransactionResponseDto
        {
            CreateTime = transaction.MerchantCreateTime ?? 0,
            // Payme's "transaction" field is the merchant's own stable identifier for this specific
            // transaction record - distinct from "account" (which already carries the order
            // reference). PaymeTransaction.Id is that identifier; it is not PaymentOrder.Id.
            Transaction = transaction.Id.ToString(),
            State = transaction.PaymeStateCode ?? PaymeCreatedStateCode
        };
    }

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
