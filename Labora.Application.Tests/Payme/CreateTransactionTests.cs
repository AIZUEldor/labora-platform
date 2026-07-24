using Labora.Application.DTOs.Payments.Payme;
using Labora.Application.Options;
using Labora.Application.Services;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Exceptions;

namespace Labora.Application.Tests.Payme;

public class CreateTransactionTests
{
    private static PaymentOrder CreateOrder(decimal amountSoum = 10_000m)
    {
        return new PaymentOrder
        {
            Id = Guid.NewGuid(),
            Amount = amountSoum,
            Provider = PaymentProvider.Payme,
            Status = PaymentOrderStatus.Pending,
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
    }

    private static (PaymeMerchantService Service, FakePaymentOrderRepository OrderRepository, FakePaymeTransactionRepository TransactionRepository) CreateService(params PaymentOrder[] seedOrders)
    {
        FakePaymentOrderRepository orderRepository = new(seedOrders);
        FakePaymeTransactionRepository transactionRepository = new();
        PaymeAuthenticator authenticator = new(Microsoft.Extensions.Options.Options.Create(new PaymeMerchantOptions
        {
            MerchantId = "merchant-1",
            Login = "Paycom",
            Password = "secret"
        }));

        PaymeMerchantService service = new(authenticator, orderRepository, transactionRepository, new FakeUserRepository(), new PassThroughUnitOfWork());
        return (service, orderRepository, transactionRepository);
    }

    private static CreateTransactionRequestDto Request(string id, Guid orderId, long amount = 1_000_000, long time = 1) => new()
    {
        Id = id,
        Time = time,
        Amount = amount,
        Account = new PaymeAccountDto { OrderId = orderId.ToString() }
    };

    [Fact]
    public async Task CreateTransactionAsync_FirstCall_CreatesTransactionAndSetsProviderOrderId()
    {
        PaymentOrder order = CreateOrder(amountSoum: 10_000m);
        (PaymeMerchantService service, FakePaymentOrderRepository orderRepository, FakePaymeTransactionRepository transactionRepository) = CreateService(order);

        CreateTransactionResponseDto response = await service.CreateTransactionAsync(Request("payme-tx-1", order.Id));

        PaymeTransaction? stored = await transactionRepository.GetByPaymeTransactionIdAsync("payme-tx-1");
        Assert.NotNull(stored);
        Assert.Equal(order.Id, stored!.PaymentOrderId);
        Assert.Equal(1_000_000, stored.RequestedAmountTiyin);
        Assert.Equal(PaymeTransactionInternalStatus.Created, stored.InternalStatus);

        // The response's "transaction" field must be PaymeMerchantService's own stable transaction
        // identifier (PaymeTransaction.Id), not PaymentOrder.Id - they are different concepts.
        Assert.Equal(stored.Id.ToString(), response.Transaction);
        Assert.NotEqual(order.Id.ToString(), response.Transaction);
        Assert.Equal(response.CreateTime, stored.MerchantCreateTime);

        PaymentOrder? updatedOrder = orderRepository.GetRaw(order.Id);
        Assert.Equal("payme-tx-1", updatedOrder!.ProviderOrderId);
    }

    [Fact]
    public async Task CreateTransactionAsync_RepeatedCallSameId_ReplaysStoredValues_NoDuplicateRow()
    {
        PaymentOrder order = CreateOrder();
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository) = CreateService(order);
        CreateTransactionRequestDto request = Request("payme-tx-1", order.Id);

        CreateTransactionResponseDto first = await service.CreateTransactionAsync(request);
        CreateTransactionResponseDto second = await service.CreateTransactionAsync(request);

        Assert.Equal(first.CreateTime, second.CreateTime);
        Assert.Equal(first.Transaction, second.Transaction);
        Assert.Equal(first.State, second.State);
        Assert.Equal(1, transactionRepository.AddAsyncCallCount);
    }

    [Fact]
    public async Task CreateTransactionAsync_ReplayWithMismatchedOrder_ThrowsOperationNotAllowed()
    {
        PaymentOrder order = CreateOrder();
        PaymentOrder otherOrder = CreateOrder();
        (PaymeMerchantService service, _, _) = CreateService(order, otherOrder);

        await service.CreateTransactionAsync(Request("payme-tx-1", order.Id));

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CreateTransactionAsync(Request("payme-tx-1", otherOrder.Id)));

        Assert.Equal(PaymeErrorCodes.OperationNotAllowed, ex.PaymeErrorCode);
    }

    [Fact]
    public async Task CreateTransactionAsync_ReplayWithMismatchedAmount_ThrowsOperationNotAllowed()
    {
        PaymentOrder order = CreateOrder();
        (PaymeMerchantService service, _, _) = CreateService(order);

        await service.CreateTransactionAsync(Request("payme-tx-1", order.Id, amount: 1_000_000));

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CreateTransactionAsync(Request("payme-tx-1", order.Id, amount: 500_000)));

        Assert.Equal(PaymeErrorCodes.OperationNotAllowed, ex.PaymeErrorCode);
    }

    [Fact]
    public async Task CreateTransactionAsync_DifferentPaymeIdClaimingSameOrder_ThrowsOperationNotAllowed()
    {
        PaymentOrder order = CreateOrder();
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository) = CreateService(order);

        await service.CreateTransactionAsync(Request("payme-tx-1", order.Id));

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CreateTransactionAsync(Request("payme-tx-2", order.Id)));

        Assert.Equal(PaymeErrorCodes.OperationNotAllowed, ex.PaymeErrorCode);
        Assert.Equal(1, transactionRepository.AddAsyncCallCount);
    }

    [Fact]
    public async Task CreateTransactionAsync_AmountMismatchOnFirstCreate_ThrowsInvalidAmount_NoPartialState()
    {
        PaymentOrder order = CreateOrder(amountSoum: 10_000m);
        (PaymeMerchantService service, FakePaymentOrderRepository orderRepository, FakePaymeTransactionRepository transactionRepository) = CreateService(order);

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CreateTransactionAsync(Request("payme-tx-1", order.Id, amount: 1)));

        Assert.Equal(PaymeErrorCodes.InvalidAmount, ex.PaymeErrorCode);
        Assert.Equal(0, transactionRepository.AddAsyncCallCount);
        Assert.Null(orderRepository.GetRaw(order.Id)!.ProviderOrderId);
    }

    [Fact]
    public async Task CreateTransactionAsync_OrderNotFound_ThrowsAccountError_NoRowCreated()
    {
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository) = CreateService();

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CreateTransactionAsync(Request("payme-tx-1", Guid.NewGuid())));

        Assert.Equal(PaymeErrorCodes.AccountErrorRangeStart, ex.PaymeErrorCode);
        Assert.Equal(0, transactionRepository.AddAsyncCallCount);
    }

    [Fact]
    public async Task CreateTransactionAsync_ConflictOnFirstAttempt_RetriesAndResolvesToReplay()
    {
        PaymentOrder order = CreateOrder();
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository) = CreateService(order);
        CreateTransactionRequestDto request = Request("payme-tx-1", order.Id);

        // Simulate a concurrent CreateTransaction for the same Payme id committing between this call's
        // idempotency read and its own AddAsync: the first AddAsync attempt hits a unique-index
        // conflict, and the row is already present by the time the retry re-reads.
        transactionRepository.FaultOnAddAsync = callNumber =>
        {
            if (callNumber != 1)
            {
                return null;
            }

            transactionRepository.SeedDirectly(new PaymeTransaction
            {
                PaymentOrderId = order.Id,
                PaymeTransactionId = request.Id,
                AccountReference = request.Account.OrderId,
                PaymeTransactionTime = request.Time,
                RequestedAmountTiyin = request.Amount,
                MerchantCreateTime = 123_456,
                InternalStatus = PaymeTransactionInternalStatus.Created,
                PaymeStateCode = 1
            });
            return new PaymeConflictException("simulated unique-index conflict");
        };

        CreateTransactionResponseDto response = await service.CreateTransactionAsync(request);

        Assert.Equal(123_456, response.CreateTime);
        Assert.Equal(1, transactionRepository.AddAsyncCallCount);
    }

    [Fact]
    public async Task CreateTransactionAsync_ConflictOnEveryAttempt_ThrowsInternalSystemErrorAfterRetriesExhausted()
    {
        PaymentOrder order = CreateOrder();
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository) = CreateService(order);

        transactionRepository.FaultOnAddAsync = _ => new PaymeConflictException("always conflicts");

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CreateTransactionAsync(Request("payme-tx-1", order.Id)));

        Assert.Equal(PaymeErrorCodes.InternalSystemError, ex.PaymeErrorCode);
        Assert.Equal(3, transactionRepository.AddAsyncCallCount);
    }
}
